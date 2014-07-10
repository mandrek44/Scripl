using System.ComponentModel;
using System.Diagnostics;
using System.IO;

using NLog;

using Scripl.Adapters;
using Scripl.NotStructured;
using Scripl.NotStructured.Commands;
using Scripl.PortsIn;
using Scripl.PortsOut;

namespace Scripl.Core
{
    public class Edit : IEdit
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly CommandRunner _runner;

        private readonly IVisualStudioHelper _visualStudio;
        private readonly IFileSystem _fileSystem;
        private readonly ITemporaryFileManager _temporaryFileManager;
        private readonly IProcessFactory _processFactory;

        public Edit(CommandRunner runner, IVisualStudioHelper visualStudio, IFileSystem fileSystem, ITemporaryFileManager temporaryFileManager, IProcessFactory processFactory)
        {
            _runner = runner;
            _visualStudio = visualStudio;
            _fileSystem = fileSystem;
            _temporaryFileManager = temporaryFileManager;
            _processFactory = processFactory;
        }

        public void EditExec(string exeName, bool forceUseDefault, bool waitForUserInput)
        {
            if (!_fileSystem.Exists(exeName))
            {
                _log.Trace("File does not exists");
                return;
            }

            var sourceFilePath = Path.ChangeExtension(_temporaryFileManager.GetTempFileName(), "cs");
            
            // Use the runner so it will automatically choose if the command should be sent to server or run locally
            _runner.Invoke("monitor", "-no-wait", "-is-temp", sourceFilePath, exeName);

            if (!forceUseDefault && _visualStudio.IsInstalled())
            {
                _visualStudio.EditSourceFile(sourceFilePath, waitForEnd: !waitForUserInput);
            }
            else
            {
                try
                {
                    _log.Trace("Opening file for editor");
                    var process = _processFactory.Start(sourceFilePath);
                    if (!waitForUserInput)
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