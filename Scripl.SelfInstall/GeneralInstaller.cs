namespace Scripl.SelfInstall
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Security.Principal;

    using Scripl.SelfInstall.Alfa.Kancelaria.Client.Console;

    using Microsoft.Win32;

    public class GeneralInstaller
    {
        public struct InstallerOptions
        {
            public bool CopyToFinalDirectory { get; set; }

            public bool CreateDataFolder { get; set; }

            public bool InstallService { get; set; }

            public static InstallerOptions Default
            {
                get
                {
                    return new InstallerOptions
                               {
                                   CopyToFinalDirectory = true, CreateDataFolder = true,
                                   InstallService = true, UninstallOldService = false
                               };
                }
            }

            public bool UninstallOldService { get; set; }
        }

        public static bool EnsureAdminCredentials(string[] arg, bool runAsAdmin = true)
        {
            if (arg == null)
            {
                arg = new string[] { };
            }

            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool administrativeMode = principal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!administrativeMode && runAsAdmin)
            {
                var startInfo = new ProcessStartInfo
                {
                    Verb = "runas",
                    FileName = Assembly.GetEntryAssembly().Location,
                    Arguments = String.Join(" ", arg),
                    UseShellExecute = false
                };

                Process.Start(startInfo);
            }

            return administrativeMode;
        }

        public static void Install(InstallerOptions options, InstallerPaths installerInstallerPaths, ServiceDescription serviceDescription)
        {
            if (options.CopyToFinalDirectory)
            {
                if (!Directory.Exists(installerInstallerPaths.ProgramFilesPath))
                {
                    // Copy files to program files folder
                    // _log.Debug("Creating Program Files Dir");
                    Directory.CreateDirectory(installerInstallerPaths.ProgramFilesPath);
                }
            }

            if (options.InstallService)
            {
                // Register and run main service
                if (ServiceInstaller.ServiceIsInstalled(serviceDescription.ServiceName))
                {
                    //_log.Debug("Removing existing service");
                    ServiceInstaller.StopService(serviceDescription.ServiceName);

                    if (options.UninstallOldService)
                        ServiceInstaller.Uninstall(serviceDescription.ServiceName);
                }
            }

            if (options.CopyToFinalDirectory)
            {
                //_log.Debug("Copying files...");
                if (options.CopyToFinalDirectory)
                {
                    DirectoryHelper.CopyDirectory(installerInstallerPaths.CurrentPath, installerInstallerPaths.ProgramFilesPath, true);
                }
            }

            if (options.CreateDataFolder)
            {
                // Copy databases to ApplicationData folder
                if (!Directory.Exists(installerInstallerPaths.RootDataPath))
                {
                    Directory.CreateDirectory(installerInstallerPaths.RootDataPath);
                }
            }

            if (options.InstallService)
            {
                //_log.Debug("Installing and running service");
                ServiceInstaller.InstallAndStart(
                    serviceDescription.ServiceName,
                    serviceDescription.Description,
                    installerInstallerPaths.TargetApplicationExePath);
            }

            //_log.Info("Installation completed");

            // Register and run updater service
            // Optionaly - update hosts file to use userfriendly name instead of localhost:1337
        }

        public static void CreateContextMenuItem(string parentName, string itemName, string displayName, string command)
        {
            var dirShell = Registry.ClassesRoot.OpenSubKey(parentName, true).OpenSubKey("shell", true);
            var newScripl = dirShell.CreateSubKey(itemName);
            newScripl.SetValue(String.Empty, displayName);
            newScripl.CreateSubKey("command").SetValue(String.Empty, command);
        }

        public static void DeleteContextMenuItem(string parentName, string itemName)
        {
            var dirShell = Registry.ClassesRoot.OpenSubKey(parentName, true).OpenSubKey("shell", true);
            var newScriplKey = dirShell.OpenSubKey(itemName);
            if (newScriplKey != null)
            {
                dirShell.DeleteSubKeyTree(itemName);
            }
        }

        public static void RegisterUninstallLink(InstallationInfo installationInfo)
        {
            var scriplUninstallRoot = Registry.LocalMachine.OpenSubKey(UninstallRegKey, true).CreateSubKey(installationInfo.Id);
            scriplUninstallRoot.SetValue(string.Empty, installationInfo.ProgramName);
            scriplUninstallRoot.SetValue("DisplayName", installationInfo.ProgramName);
            scriplUninstallRoot.SetValue("Publisher", installationInfo.Publisher);
            scriplUninstallRoot.SetValue("InstallLocation", installationInfo.Paths.ProgramFilesPath);
            scriplUninstallRoot.SetValue("UninstallString", installationInfo.UninstallCommand);
        }

        public static void UnregisterUninstallLink(InstallationInfo installationInfo)
        {
            Registry.LocalMachine.OpenSubKey(UninstallRegKey, true).DeleteSubKey(installationInfo.Id);
        }

        private static string UninstallRegKey
        {
            get
            {
                return Wow64Helper.Is64BitOperatingSystem
                           ? @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
                           : @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
            }
        }
    }
}