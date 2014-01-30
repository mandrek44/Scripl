using System;
using System.Collections.Generic;
using System.Text;

namespace Scripl.RecompilerService
{
    using System.Diagnostics;
    using System.Reflection;
    using System.ServiceProcess;

    using Scripl.SelfInstall;

    using NLog;

    using ServiceStack.Common;
    using ServiceStack.Text;

    using ServiceInstaller = Scripl.SelfInstall.ServiceInstaller;

    class Program
    {
        private static readonly Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            var paths = InstallationInfo.Default.Paths;
            AppDomain.CurrentDomain.SetData("DataDirectory", paths.RootDataPath);

            _log.Debug("Starting Scripl Service Exe");
            _log.Trace(args.Dump());
            _log.Trace(paths.Dump());

            if (Environment.UserInteractive)
            {
                if (args.Length == 1 && args[0] == "run")
                {
                    new WindowsService(paths).Start();

                    Console.WriteLine("Services started");
                    Console.ReadKey();
                }
                else if (args.Length == 1 && args[0] == "uninstall")
                {
                    ServiceInstaller.StopService("ScriplService");
                    ServiceInstaller.Uninstall("ScriplService");
                }
                else
                {
                    if (GeneralInstaller.EnsureAdminCredentials(args))
                    {
                        _log.Trace("Installing...");
                        
                        GeneralInstaller.Install(GeneralInstaller.InstallerOptions.Default, paths, new ServiceDescription() { ServiceName = "ScriplService", Description = "Scripl Service" });

                        _log.Trace("Install completed");
                    }
                }
            }
            else
            {
                ServiceBase.Run(new WindowsService(paths));
            }
        }
    }
}
