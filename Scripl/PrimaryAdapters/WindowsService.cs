using System;
using System.Reflection;
using System.ServiceProcess;

using Autofac;

using NLog;

using Scripl.Adapters;

namespace Scripl.NotStructured
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
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).AsSelf().AsImplementedInterfaces();
            containerBuilder.RegisterInstance(new SourceCodeRepository());
            containerBuilder.RegisterInstance(this).AsSelf();
            
            _log.Trace("Starting service");
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
