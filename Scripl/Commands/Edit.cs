using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Scripl.Commands
{
    [Command("edit")]
    public class Edit
    {
        private bool _vsInstalled = false;

        private CommandRunner _runner;

        public Edit(CommandRunner runner)
        {
            _runner = runner;
        }

        public void Run(string exeName)
        {
            if (!File.Exists(exeName))
            {
                Console.WriteLine("File does not exists");
                return;
            }

            var sourceFilePath = Path.ChangeExtension(Path.GetTempFileName(), "cs");

            _runner.Invoke("monitor", sourceFilePath, exeName);
            
            if (_vsInstalled)
            {
                EditWithVisualStudio(sourceFilePath, new TempFileCollection());
            }
            else
            {
                try
                {
                    Console.WriteLine("Opening file for editor");
                    Process.Start(sourceFilePath).WaitForExit();
                }
                catch (Win32Exception ex)
                {
                    Console.WriteLine("Exception caught " + ex);
                    //GeneralInstaller.EnsureAdminCredentials(new[] { "edit", exeName });
                }
            }
        }

        private static void EditWithVisualStudio(string sourceFile, TempFileCollection tempFileCollection)
        {
            //string initialFile = Path.Combine(_installationInfo.Paths.CurrentPath, "csprojTemplate.xml");

            //var csProjFileContent = File.ReadAllText(initialFile).Replace("{sourceFileName}", Path.GetFileName(sourceFile));
            //var tempCSProjFile = tempFileCollection.AddExtension("csproj", keepFile: true);
            //File.WriteAllText(tempCSProjFile, csProjFileContent);

            //Process.Start(tempCSProjFile);
        }
    }
}