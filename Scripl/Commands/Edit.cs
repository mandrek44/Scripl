using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Win32;

using NDesk.Options;

namespace Scripl.Commands
{
    [Command("edit")]
    public class Edit
    {
        private readonly CommandRunner _runner;

        public Edit(CommandRunner runner)
        {
            _runner = runner;
        }

        public bool IsVisualStudioInstalled()
        {
            var regex = new Regex(@"^VisualStudio\.edmx\.(\d+)\.(\d+)$");
            return Registry.ClassesRoot.GetSubKeyNames().Any(regex.IsMatch);
        }


        public void Run(params string[] args)
        {
            bool forceUseDefault = false;
            bool waitForUserInput = false;
            var unparsedArgs = (new OptionSet
                                {
                                    { "d|default", v=>forceUseDefault = v != null },
                                    { "w|wait", v=>waitForUserInput = v != null}
                                }).Parse(args);
            var exeName = unparsedArgs[0];

            if (!File.Exists(exeName))
            {
                Console.WriteLine("File does not exists");
                return;
            }

            var sourceFilePath = Path.ChangeExtension(Path.GetTempFileName(), "cs");

            _runner.Invoke("monitor", "-no-wait", "-is-temp", sourceFilePath, exeName);

            if (!forceUseDefault && IsVisualStudioInstalled())
            {
                EditWithVisualStudio(sourceFilePath, new TempFileCollection(), waitForUserInput);
            }
            else
            {
                try
                {
                    Console.WriteLine("Opening file for editor");
                    var process = Process.Start(sourceFilePath);
                    if (!waitForUserInput)
                        process.WaitForExit();
                }
                catch (Win32Exception ex)
                {
                    Console.WriteLine("Exception caught " + ex);
                }
            }

            if (waitForUserInput)
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        private static void EditWithVisualStudio(string sourceFile, TempFileCollection tempFileCollection, bool waitForUserInput)
        {
            var csProjFileContent = Properties.Settings.Default.csprojTemplate.Replace("{sourceFileName}", Path.GetFileName(sourceFile));
            var tempCSProjFile = tempFileCollection.AddExtension("csproj", keepFile: true);
            File.WriteAllText(tempCSProjFile, csProjFileContent);

            var slnContent = Properties.Settings.Default.slnTemplate
                .Replace("{SolutionGuid}", Guid.NewGuid().ToString())
                .Replace("{ProjectGuid}", Guid.NewGuid().ToString())
                .Replace("{projectName}", Path.GetFileNameWithoutExtension(tempCSProjFile))
                .Replace("{csprojPath}", tempCSProjFile);

            var slnFile = tempFileCollection.AddExtension("sln", keepFile: true);
            File.WriteAllText(slnFile, slnContent);

            var process = Process.Start(slnFile);
            if (!waitForUserInput)
                process.WaitForExit();
        }
    }
}