using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

using Autofac;

using NLog;

using Scripl.Commands;

namespace Scripl
{
    public class CommandRunner
    {
        private static Logger _log = NLog.LogManager.GetCurrentClassLogger();
        private IContainer _iocContainer;
        private readonly bool _isService;
        private static readonly object _iocContainerSync = new object();

        private IDictionary<string, Type> _commands;

        private IServiceAddressProvider _addressProvider;

        public CommandRunner()
        {
            _isService = false;
        }

        public CommandRunner(bool isService)
        {
            _isService = isService;
        }

        public void Invoke(string commandName, params string[] commandArgs)
        {
            var commandType = Commands[commandName];

            if (!IsService && commandType.GetCustomAttributes(typeof(RunOnServiceAttribute)).Any() && IsServiceRunning())
            {
                RunOnService(commandName, commandArgs);
            }
            else
            {
                Invoke(commandType, commandArgs);
            }
        }

        private IContainer IocContainer
        {
            get
            {
                if (_iocContainer == null)
                {
                    lock (_iocContainerSync)
                    {
                        if (_iocContainer == null)
                        {
                            var containerBuilder = new ContainerBuilder();
                            containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).AsSelf().AsImplementedInterfaces();
                            containerBuilder.RegisterInstance(this).AsSelf();
                            _iocContainer = containerBuilder.Build();
                        }
                    }
                }

                return _iocContainer;
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

        private void Invoke(Type commandType, string[] commandArgs)
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

            methodInfo.Invoke(IocContainer.Resolve(commandType), realArgs);
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

        private void RunOnService(string commandName, string[] commandArgs)
        {
            using (var wc = new WebClient())
            {
                var data = new NameValueCollection();
                for (int i = 0; i < commandArgs.Length; i++)
                {
                    data.Add(i.ToString(), commandArgs[i]);
                }

                wc.UploadValues(string.Format("{0}/cli/", AddressProvider.GetAddress()) + commandName, "POST", data);
            }
        }
    }
}