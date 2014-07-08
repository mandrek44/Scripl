using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Win32;

using NDesk.Options;

using NLog;

namespace Scripl.Commands
{
    [Command("edit")]
    public class Edit
    {
        private static Logger _log = NLog.LogManager.GetCurrentClassLogger();
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

            var exeName = Path.GetFullPath(unparsedArgs[0]);  

            if (!File.Exists(exeName))
            {
                _log.Trace("File does not exists");
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
                    _log.Trace("Opening file for editor");
                    var process = Process.Start(sourceFilePath);
                    if (!waitForUserInput)
                        process.WaitForExit();
                }
                catch (Win32Exception ex)
                {
                    _log.Trace("Exception caught " + ex);
                }
            }

            if (waitForUserInput)
            {
                _log.Trace("Press any key to exit");
                Console.Read();
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