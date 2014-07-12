using System.CodeDom.Compiler;
using System.ComponentModel;
using System.IO;

using NDesk.Options;

using NLog;

using Scripl.PortsIn;
using Scripl.SecondaryAdapters.OS;

using Process = System.Diagnostics.Process;

namespace Scripl.PrimaryAdapters.Console
{
    [Command("edit")]
    public class Edit
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();        

        private readonly CommandRunner _runner;
        private readonly VisualStudioHelper _visualStudio;
        private readonly ISourceCode _sourceCode;

        public Edit(ISourceCode sourceCode, CommandRunner runner, VisualStudioHelper visualStudio)
        {
            _sourceCode = sourceCode;
            _runner = runner;
            _visualStudio = visualStudio;
        }

        public void Run(params string[] args)
        {
            // Parse input args
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
                _log.Error("File " + exeName + " doesn't exist.");
                return;
            }

            // Create temporary file
            using (var tempFileCollection = new TempFileCollection())
            {
                var sourceFilePath = tempFileCollection.AddExtension("cs");
                File.WriteAllText(sourceFilePath, _sourceCode.GetSourceCode(exeName));

                // Monitor and edit
                _runner.Invoke((Monitor monitor) => monitor.Run("-no-wait", sourceFilePath, exeName));
                RunEditor(sourceFilePath, forceUseDefault, waitForEnd: !waitForUserInput);

                if (waitForUserInput)
                {
                    _log.Trace("Press any key to exit");
                    System.Console.Read();
                }
            }
        }

        public void RunEditor(string sourceFilePath, bool forceUseDefault, bool waitForEnd)
        {
            if (!forceUseDefault && _visualStudio.IsInstalled())
            {
                _visualStudio.EditSourceFile(sourceFilePath, waitForEnd);
            }
            else
            {
                try
                {
                    _log.Trace("Opening file for editor");
                    var process = Process.Start(sourceFilePath);
                    if (!waitForEnd)
                    {
                        process.WaitForExit();
                    }
                }
                catch (Win32Exception ex)
                {
                    _log.Trace("Exception caught " + ex);
                }
            }
        }
    }
}