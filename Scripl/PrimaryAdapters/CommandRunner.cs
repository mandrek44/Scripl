using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;

using Autofac;

using NLog;

using Scripl.PortsOut;
using Scripl.SecondaryAdapters;

namespace Scripl.PrimaryAdapters
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
                    var containerBuilder = new ContainerBuilder();
                    containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).AsSelf().AsImplementedInterfaces();
                    containerBuilder.RegisterInstance(new SourceCodeRepository()).AsSelf().AsImplementedInterfaces();
                    containerBuilder.RegisterInstance(this).AsSelf();
                    return containerBuilder.Build();
                });
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

        public void Invoke<T>(Expression<Action<T>> exp)
        {
            var commandName = exp.Parameters.First().Type.GetCustomAttribute<CommandAttribute>().Name;
            var arguments = ((MethodCallExpression)exp.Body).Arguments;

            Invoke(commandName, arguments.OfType<ConstantExpression>().Select(arg => arg.Value.ToString()).ToArray());
        }

        private IContainer IocContainer
        {
            get
            {
                return _container.Value;
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
                return _addressProvider ?? (_addressProvider = IocContainer.Resolve<IServiceAddressProvider>());
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

            return methodInfo.Invoke(IocContainer.Resolve(commandType), realArgs);
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