using System.IO;

using NDesk.Options;

using NLog;

using Scripl.SelfInstall;

namespace Scripl.Commands
{
    [Command("setup")]
    public class Setup
    {
        private static Logger _log = NLog.LogManager.GetCurrentClassLogger();

        public void Run(params string[] args)
        {
            bool installService = false;
            bool remove = false;
            new OptionSet
            {
                { "service", v => installService = v != null },
                { "remove" , v => remove = v !=  null }
            }.Parse(args);

            if (installService)
            {
                if (!remove)
                {
                    var installerOptions = new GeneralInstaller.InstallerOptions { CopyToFinalDirectory = false, CreateDataFolder = true, InstallService = true, UninstallOldService = true };

                    var paths = InstallationInfo.Default.Paths;
                    paths.ProgramFilesPath = paths.CurrentPath;
                    paths.TargetApplicationExePath = Path.Combine(paths.CurrentPath, paths.ApplicationExecName);

                    var serviceDescription = new ServiceDescription { Description = "Scripl Recompiler Server", ServiceName = "ScriplService" };

                    GeneralInstaller.Install(installerOptions, paths, serviceDescription);
                    _log.Debug("Service installed");
                }
                else
                {
                    ServiceInstaller.Uninstall("ScriplService");
                    _log.Debug("Service removed");

                }
            }
        }
    }
}