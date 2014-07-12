using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using NLog;

using Scripl.Ports;
using Scripl.Utils.Contracts;

namespace Scripl.Core
{
    internal class Monitor : IMonitor
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly ICompiler _compiler;
        private readonly IFileSystem _fileSystem;

        private readonly ISourceCode _sourceCode;

        public Monitor(ICompiler compiler, IFileSystem fileSystem, ISourceCode sourceCode)
        {
            _compiler = compiler;
            _fileSystem = fileSystem;
            _sourceCode = sourceCode;
        }

        public void StartRecompilingOnChange(string targetExec, string sourceCodeFile)
        {
            if (!_fileSystem.Exists(targetExec))
            {
                _log.Trace(targetExec + " does not exists");
                throw new InvalidOperationException();
            }


            var sourceCode = _sourceCode.GetSourceCode(targetExec);
            if (sourceCode == null)
            {
                _log.Trace("Cannot find sources for " + targetExec);
                throw new FileNotFoundException();
            }

            _fileSystem.WriteAllText(sourceCodeFile, sourceCode);

            var token = _cancellationTokenSource.Token;
            Action recompile = () => RecompileFile(sourceCodeFile, targetExec);

            Task.Run(
                () =>
                {
                    var fileSystemWatcher = _fileSystem.WatchFile(sourceCodeFile);

                    fileSystemWatcher.Changed += (sender, _) => recompile();
                    fileSystemWatcher.Renamed += (sender, _) => recompile();
                    fileSystemWatcher.Created += (sender, _) => recompile();

                    fileSystemWatcher.EnableRaisingEvents = true;

                    _log.Trace("Waiting for changes in " + sourceCodeFile);
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
                _sourceCode.Save(_fileSystem.ReadAllTextRetrying(sourceCodeFile), targetExec);
            }
        }
    }
}