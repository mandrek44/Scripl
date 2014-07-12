using System;
using System.ServiceProcess;

using Autofac;

using NLog;

namespace Scripl.Runtime
{
    partial class WindowsService : ServiceBase
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private IDisposable _service;

        public WindowsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Start(args);
        }

        public void Start(string[] args)
        {
            _log.Trace("Starting service");
            var containerBuilder = CommandRunner.GetContainerBuilder();
            containerBuilder.RegisterInstance(new CommandRunner(isService: true)).AsSelf().AsImplementedInterfaces();

            _service = containerBuilder.Build().Resolve<NetworkEndpoint>().Start();
        }

        protected override void OnStop()
        {
            _log.Trace("Stopping service");

            if (_service != null)
            {
                _service.Dispose();
                _service = null;
            }
        }
    }
}
