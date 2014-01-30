using System;

namespace Scripl.ConsoleHost
{
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    using Scripl.SelfInstall;

    using Microsoft.Win32;

    using NLog;

    using Scripl.RecompilerService.Contract;

    using ServiceStack.ServiceClient.Web;

    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);


        private static bool _vsInstalled = true;

        static Logger _log = LogManager.GetCurrentClassLogger();

        private static InstallationInfo _installationInfo = InstallationInfo.Default;

        static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "interactive")
            {
                RunInteractiveMode();
            }
            else
            {
                ParseArgsAndExecute(args);
            }
        }

        private static void RunInteractiveMode()
        {
            string[] args;
            InitConsole();

            while (true)
            {
                Console.Write("> ");
                args = Console.ReadLine().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                ParseArgsAndExecute(args);
            }
        }

        private static void InitConsole()
        {
            IntPtr ptr = GetForegroundWindow();
            int u;
            GetWindowThreadProcessId(ptr, out u);
            Process process = Process.GetProcessById(u);

            
            if (process.ProcessName == "cmd") //Is the uppermost window a cmd process?
            {
                AttachConsole(process.Id);
            }
            else
            {
                //no console AND we're in console mode ... create a new console.
                AllocConsole();
            }

            Console.WriteLine("ProcessName - " + process.ProcessName);
        }

        private static void ParseArgsAndExecute(string[] args)
        {
            if (args.Length == 2 && args[0] == "new")
            {
                CreateNewScriplFile(args[1]);
            }
            else if (args.Length == 2 && args[0] == "edit")
            {
                EditScriplFile(args[1]);
            }
            else if (args.Length == 1 && args[0] == "register")
            {
                if (!GeneralInstaller.EnsureAdminCredentials(args)) return;

                GeneralInstaller.CreateContextMenuItem("exefile", "EditScripl", "Edit", string.Format("\"{0}\" edit \"%1\"", Assembly.GetEntryAssembly().Location));
                GeneralInstaller.CreateContextMenuItem(@"Directory\Background", "NewScripl", "New Scripl", string.Format("\"{0}\" new \"%V\"", Assembly.GetEntryAssembly().Location));
                GeneralInstaller.CreateContextMenuItem("Folder", "NewScripl", "New Scripl", string.Format("\"{0}\" new \"%1\"", Assembly.GetEntryAssembly().Location));
                
                GeneralInstaller.RegisterUninstallLink(_installationInfo);
            }
            else if (args.Length == 1 && args[0] == "unregister")
            {
                if (!GeneralInstaller.EnsureAdminCredentials(args)) return;

                GeneralInstaller.DeleteContextMenuItem("exefile", "EditScripl");
                GeneralInstaller.DeleteContextMenuItem("Directory\\Background", "NewScripl");
                GeneralInstaller.DeleteContextMenuItem("Folder", "NewScripl");
                GeneralInstaller.UnregisterUninstallLink(_installationInfo);
            }
            else if (args.Length == 0)
            {
                Console.WriteLine("Installing Scripl...");
                Install(args);
                Console.WriteLine("Scripl Installed");
            }
            else if (args.Length == 1 && args[0] == "uninstall")
            {
                if (!GeneralInstaller.EnsureAdminCredentials(args)) return;

                var programFilesScripl = Path.Combine(_installationInfo.Paths.ProgramFilesPath, "Scripl.ConsoleHost.exe");

                if (File.Exists(programFilesScripl))
                {
                    Process.Start(
                        new ProcessStartInfo
                            { FileName = programFilesScripl, Arguments = "unregister", UseShellExecute = false }).WaitForExit();
                }

                Process.Start(new ProcessStartInfo{ FileName = "Scripl.RecompilerService.exe", Arguments = "uninstall", UseShellExecute = false }).WaitForExit();

                if (Directory.Exists(_installationInfo.Paths.ProgramFilesPath))
                {
                    if (!AreEqual(_installationInfo.Paths.ProgramFilesPath, Path.GetDirectoryName(typeof(Program).Assembly.Location)))
                        new DirectoryInfo(_installationInfo.Paths.ProgramFilesPath).Delete(true);
                    else
                    {
                        Process.Start(
                            new ProcessStartInfo(
                                "cmd.exe",
                                "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & RD /S /Q \"" + _installationInfo.Paths.ProgramFilesPath
                                + "\"") { UseShellExecute = false, CreateNoWindow = true});
                    }
                }
            }
        }

        public static bool AreEqual(string dir1, string dir2)
        {
            var dirUserSelected = new DirectoryInfo(dir1);
            var dirWorkingFolder = new DirectoryInfo(dir2);

            if (dirUserSelected.FullName == dirWorkingFolder.FullName)
            {
                // this will be skipped, 
                // since the first string contains an ending "\" and the other doesn't
                // and the casing in the second differs from the first
                return true;
            }

            // to be sure all things are equal; 
            // either build string like this (or strip last char if its a separator) 
            // and compare without considering casing (or ToLower when constructing)
            var strA = Path.Combine(dirUserSelected.Parent.FullName, dirUserSelected.Name);
            var strB = Path.Combine(dirWorkingFolder.Parent.FullName, dirWorkingFolder.Name);
            return strA.Equals(strB, StringComparison.CurrentCultureIgnoreCase);

        }

        private static void Install(string[] args)
        {
            if (!GeneralInstaller.EnsureAdminCredentials(args))
            {
                return;
            }

            InitConsole();

            _log.Trace("Copying files and registering services...");
            Process.Start(new ProcessStartInfo("Scripl.RecompilerService.exe") { UseShellExecute = false, CreateNoWindow = true}).WaitForExit();

            _log.Trace("Registering user interface...");
            Process.Start(new ProcessStartInfo(Path.Combine(_installationInfo.Paths.ProgramFilesPath, "Scripl.ConsoleHost.exe"), "register") { UseShellExecute = false });

            _log.Trace("Finished.");
        }

        private static void EditScriplFile(string exeName)
        {
            if (!File.Exists(exeName))
            {
                _log.Trace("File does not exists");
                return;
            }

            var sourceFilePath = Path.ChangeExtension(Path.GetTempFileName(), "cs");

            ScriplFileEditResult scriplFileEditResult;
            using (var serviceClient = new JsonServiceClient("http://localhost:4751"))
            {
                scriplFileEditResult = serviceClient.Post(new EditScriplFile { ExeFilePath = exeName, TemporaryFile = sourceFilePath});
            }

            if (scriplFileEditResult.Status == EditScriplStatus.Success)
            {
                if (_vsInstalled)
                {
                    EditWithVisualStudio(scriplFileEditResult.SourceFilePath, new TempFileCollection());
                }
                else
                {
                    try
                    {
                        _log.Trace("Opening file for editor");
                        Process.Start(sourceFilePath);
                    }
                    catch (Win32Exception)
                    {
                        _log.Trace("Exception caught, checking for Admin Credentials");
                        GeneralInstaller.EnsureAdminCredentials(new[] { "edit", exeName });
                    }                    
                }
            }
            else
            {
                _log.Warn("Error: " + scriplFileEditResult.Status.ToString());
            }

        }

        private static void EditWithVisualStudio(string sourceFile, TempFileCollection tempFileCollection)
        {
            string initialFile = Path.Combine(_installationInfo.Paths.CurrentPath, "csprojTemplate.xml");

            var csProjFileContent = File.ReadAllText(initialFile).Replace("{sourceFileName}", Path.GetFileName(sourceFile));
            var tempCSProjFile = tempFileCollection.AddExtension("csproj", keepFile: true);
            File.WriteAllText(tempCSProjFile, csProjFileContent);

            Process.Start(tempCSProjFile);
        }

        private static void CreateNewScriplFile(string filePath)
        {
            if (!filePath.EndsWith("exe", StringComparison.InvariantCultureIgnoreCase))
            {
                filePath = Path.Combine(filePath, "Scripl.exe");
            }

            using (var serviceClient = new JsonServiceClient("http://localhost:4751"))
            {
                serviceClient.Post(new CreateScriplFile { ExecPath = filePath });
            }
        }
    }
}
