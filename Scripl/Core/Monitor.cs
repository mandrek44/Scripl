using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using NLog;

using Scripl.PortsIn;
using Scripl.PortsOut;

namespace Scripl.Core
{
    public class Monitor : IMonitor
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly ISourceCodeRepository _repository;
        private readonly ICompiler _compiler;
        private readonly IFileSystem _fileSystem;
        private readonly ITemporaryFileManager _tempFiles;

        private readonly IAddSourceCode _addSourceCode;

        public Monitor(ISourceCodeRepository repository, ICompiler compiler, IFileSystem fileSystem, ITemporaryFileManager tempFiles, IAddSourceCode addSourceCode)
        {
            _repository = repository;
            _compiler = compiler;
            _fileSystem = fileSystem;
            _tempFiles = tempFiles;
            _addSourceCode = addSourceCode;
        }

        public void Run(string targetExec, string sourceCodeFile, bool isTemp)
        {
            if (!_fileSystem.Exists(targetExec))
            {
                _log.Trace(targetExec + " does not exists");
                throw new InvalidOperationException();
            }

            var checksum = _fileSystem.GetChecksum(targetExec);
            var sourceCode = _repository.LoadSourceCode(checksum);

            if (sourceCode == null)
            {
                _log.Trace("Cannot find sources for " + targetExec);
                throw new FileNotFoundException();
            }

            var source = sourceCode.Source;
            var temporaryFile = sourceCodeFile;
            if (isTemp)
            {
                _tempFiles.AddFile(temporaryFile);
            }

            _fileSystem.WriteAllText(temporaryFile, source);

            var token = _cancellationTokenSource.Token;

            Action recompile = () => RecompileFile(temporaryFile, targetExec);

            Task.Run(
                () =>
                {
                    var fileSystemWatcher = _fileSystem.WatchFile(temporaryFile);

                    fileSystemWatcher.Changed += (sender, _) => recompile();
                    fileSystemWatcher.Renamed += (sender, _) => recompile();
                    fileSystemWatcher.Created += (sender, _) => recompile();

                    fileSystemWatcher.EnableRaisingEvents = true;

                    _log.Trace("Waiting for changes in " + temporaryFile);
                    while (!token.IsCancellationRequested)
                    {
                        fileSystemWatcher.WaitForChanged(WatcherChangeTypes.All, 500);
                    }
                },
                token);
        }

        private void RecompileFile(string sourceCodeFile, string targetExec)
        {
            _log.Trace("Recompiling " + sourceCodeFile + " to " + targetExec);
            var result = _compiler.CompileFile(targetExec, sourceCodeFile);

            if (!result.Errors.HasErrors)
            {
                _addSourceCode.Run(sourceCodeFile, targetExec);
            }
        }
    }
}