using System.IO;
using System.Reflection;

using Mandro.Utils.Setup;

using NDesk.Options;

using NLog;

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
            bool installShellExtension = false;
            new OptionSet
            {
                { "service", v => installService = v != null },
                { "shell", v => installShellExtension = v != null },
                { "remove" , v => remove = v !=  null }
            }.Parse(args);

            var installationInfo = GetInstallationInfo();
            if (installService)
            {
                if (!remove)
                {
                    var installerOptions = new GeneralInstaller.InstallerOptions { CopyToFinalDirectory = false, CreateDataFolder = true, InstallService = true, UninstallOldService = true };

                    var serviceDescription = new ServiceDescription { Description = "Scripl Recompiler Server", ServiceName = "ScriplService" };

                    GeneralInstaller.Install(installerOptions, installationInfo.Paths, serviceDescription);
                    _log.Debug("Service installed");

                    GeneralInstaller.RegisterUninstallLink(installationInfo);
                }
                else
                {
                    ServiceInstaller.Uninstall("ScriplService");
                    _log.Debug("Service removed");

                }
            }

            if (installShellExtension)
            {
                if (!remove)
                {
                    var execLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "scriplw.exe");
                    GeneralInstaller.CreateContextMenuItem("exefile", "EditScripl", "Edit", string.Format("\"{0}\" edit \"%1\"", execLocation));
                    GeneralInstaller.CreateContextMenuItem(@"Directory\Background", "NewScripl", "New Scripl", string.Format("\"{0}\" new \"%V\"", execLocation));
                    GeneralInstaller.CreateContextMenuItem("Folder", "NewScripl", "New Scripl", string.Format("\"{0}\" new \"%1\"", execLocation));

                    GeneralInstaller.RegisterUninstallLink(installationInfo);
                }
                else
                {
                    GeneralInstaller.DeleteContextMenuItem("exefile", "EditScripl");
                    GeneralInstaller.DeleteContextMenuItem("Directory\\Background", "NewScripl");
                    GeneralInstaller.DeleteContextMenuItem("Folder", "NewScripl");
                }
            }
        }

        private static InstallationInfo GetInstallationInfo()
        {
            var installationInfo = InstallationInfo.Default;
            var paths = installationInfo.Paths;
            paths.ProgramFilesPath = paths.CurrentPath;
            paths.TargetApplicationExePath = Path.Combine(paths.CurrentPath, paths.ApplicationExecName);

            installationInfo.UninstallCommand = string.Format("\"{0}\" setup -remove -service -shell", Assembly.GetEntryAssembly().Location);
            return installationInfo;
        }

    }
}