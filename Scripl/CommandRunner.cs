using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

using Autofac;

namespace Scripl
{
    public class CommandRunner
    {
        private IContainer _iocContainer;
        private readonly bool _isService;
        private static readonly object _iocContainerSync = new object();

        private string _serverBaseAddress = "http://localhost:12345";

        private IDictionary<string, Type> _commands;

        public CommandRunner()
        {
            _isService = false;
        }

        public CommandRunner(bool isService)
        {
            _isService = isService;
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

        public void Invoke(string commandName, params string[] commandArgs)
        {
            var commandType = Commands[commandName];

            if (!_isService && commandType.GetCustomAttributes(typeof(RunOnServiceAttribute)).Any() && IsServiceRunning())
            {
                RunOnService(commandName, commandArgs);
            }
            else
            {
                Invoke(commandType, commandArgs);    
            }
        }

        private void Invoke(Type commandType, string[] commandArgs)
        {
            Console.WriteLine(commandType.Name + " " + string.Join(" ", commandArgs));
            commandType.GetMethod("Run").Invoke(IocContainer.Resolve(commandType), commandArgs);
        }

        private bool IsServiceRunning()
        {
            try
            {
                using (var wc = new WebClient())
                {
                    return Encoding.Default.GetString(wc.DownloadData(string.Format("{0}/running", _serverBaseAddress))) == "OK";
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

                wc.UploadValues(string.Format("{0}/cli/", _serverBaseAddress) + commandName, "POST", data);
            }
        }
    }
}