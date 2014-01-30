using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Scripl.RecompilerService
{
    using Scripl.SelfInstall;

    partial class WindowsService : ServiceBase
    {
        private readonly InstallerPaths _paths;

        private AppHost _appHost;

        public WindowsService(InstallerPaths paths)
        {
            _paths = paths;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Start();
        }

        public void Start()
        {
            _appHost = new AppHost(_paths);
            _appHost.Init();
            _appHost.Start("http://*:4751/");
        }

        protected override void OnStop()
        {
            _appHost.Stop();
            RecompilerService.CancellationTokenSource.Cancel();
        }
    }
}
