using System;
using System.ServiceProcess;

using NLog;

using Scripl.Commands;

namespace Scripl
{
    partial class WindowsService : ServiceBase
    {
        private static Logger _log = NLog.LogManager.GetCurrentClassLogger();
        private CommandRunner _commandRunner;

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
            _service = new RunScriplService(new LocalhostAddressProvider()).Start();
        }

        protected override void OnStop()
        {
            _log.Trace("Stoppings service");

            if (_service != null)
            {
                _service.Dispose();
                _service = null;
            }
        }
    }
}
