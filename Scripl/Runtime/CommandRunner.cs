using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;

using Autofac;

using NLog;

using Scripl.Attributes;
using Scripl.Core;
using Scripl.RavenDb;
using Scripl.Utils.Contracts;

namespace Scripl.Runtime
{
    public class CommandRunner
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly bool _isService;

        private IDictionary<string, Type> _commands;
        private readonly Lazy<IContainer> _container;

        private IServiceAddressProvider _addressProvider;

        public CommandRunner()
            : this(isService: false)
        {
        }

        public CommandRunner(bool isService)
        {
            _isService = isService;
            _container = new Lazy<IContainer>(
                () =>
                {
                    var containerBuilder = GetContainerBuilder();
                    containerBuilder.RegisterInstance(this).AsSelf();
                    return containerBuilder.Build();
                });
        }

        public static ContainerBuilder GetContainerBuilder()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly(), typeof(ICompiler).Assembly).AsSelf().AsImplementedInterfaces();
            containerBuilder.RegisterScriplCore();
            containerBuilder.RegisterType<SourceCodeRepository>().InstancePerLifetimeScope().AsSelf().AsImplementedInterfaces();

            return containerBuilder;
        }

        public object Invoke(string commandName, params string[] commandArgs)
        {
            var commandType = Commands[commandName];

            if (!IsService && commandType.GetCustomAttributes(typeof(RunOnServiceAttribute)).Any() && IsServiceRunning())
            {
                return RunOnService(commandName, commandArgs);
            }
            else
            {
                return Invoke(commandType, commandArgs);
            }
        }

        public object Invoke<T>(Expression<Action<T>> exp) where T : class
        {
            var commandName = exp.Parameters.First().Type.GetCustomAttribute<CommandAttribute>().Name;
            var arguments = ((MethodCallExpression)exp.Body).Arguments;

            return Invoke(commandName, arguments.SelectMany(GetValue).ToArray());
        }

        private IEnumerable<string> GetValue(Expression member)
        {
            if (member is NewArrayExpression)
            {
                foreach (var result in ((NewArrayExpression)member).Expressions.SelectMany(GetValue))
                {
                    yield return result;
                }
            }
            else if (member is ConstantExpression)
            {
                yield return ((ConstantExpression)member).Value.ToString();
            }
            else 
            {
                var objectMember = Expression.Convert(member, typeof(string));
                var getterLambda = Expression.Lambda<Func<string>>(objectMember);

                yield return getterLambda.Compile()();
            }
        }

        private IDictionary<string, Type> Commands
        {
            get
            {
                return _commands ?? (_commands = CommandsScanner.FindCommands<CommandAttribute>(Assembly.GetExecutingAssembly()));
            }
        }

        public bool IsService
        {
            get
            {
                return _isService;
            }
        }

        public IServiceAddressProvider AddressProvider
        {
            get
            {
                return _addressProvider ?? (_addressProvider = _container.Value.Resolve<IServiceAddressProvider>());
            }
        }

        private object Invoke(Type commandType, string[] commandArgs)
        {
            _log.Trace(commandType.Name + " " + string.Join(" ", commandArgs));
            var methodInfo = commandType.GetMethod("Run");
            object[] realArgs;
            if (methodInfo.GetParameters().Count() == 1 && methodInfo.GetParameters().First().GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
            {
                realArgs = new object[] { commandArgs };
            }
            else
            {
                realArgs = commandArgs;
            }

            return methodInfo.Invoke(_container.Value.Resolve(commandType), realArgs);
        }

        private bool IsServiceRunning()
        {
            try
            {
                using (var wc = new WebClient())
                {
                    return Encoding.Default.GetString(wc.DownloadData(string.Format("{0}/running", AddressProvider.GetAddress()))) == "OK";
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string RunOnService(string commandName, string[] commandArgs)
        {
            using (var wc = new WebClient())
            {
                var data = new NameValueCollection();
                for (var i = 0; i < commandArgs.Length; i++)
                {
                    data.Add(i.ToString(), commandArgs[i]);
                }

                return Encoding.UTF8.GetString(wc.UploadValues(string.Format("{0}/cli/", AddressProvider.GetAddress()) + commandName, "POST", data));
            }
        }
    }
}