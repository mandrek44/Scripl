using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using NDesk.Options;

using NLog;

using Scripl.Data;

namespace Scripl.Commands
{
    [Command("monitor")]
    [RunOnService]
    public class Monitor
    {
        private static Logger _log = NLog.LogManager.GetCurrentClassLogger();
        private static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private static readonly TempFileCollection _tempFiles = new TempFileCollection();

        public void Run(params string[] args)
        {
            bool wait = true;
            bool isTemp = false;
            var notParsedArgs = (new OptionSet
                                 {
                                     { "no-wait", v => wait = v == null },
                                     { "is-temp", v => isTemp = v != null }
                                 }).Parse(args);
            var targetExec = notParsedArgs[1];
            var sourceCodeFile = notParsedArgs[0];

            if (!File.Exists(targetExec))
            {
                _log.Trace(targetExec + " does not exists");
                throw new InvalidOperationException();
            }

            var checksum = FileHelper.GetChecksum(targetExec);

            var sourceCode = SourceCodeRepository.Instance.LoadSourceCode(checksum);

            if (sourceCode == null)
            {
                _log.Trace("Cannot find sources for " + targetExec);
                throw new FileNotFoundException();
            }

            var source = sourceCode.Source;
            var temporaryFile = sourceCodeFile;
            if (isTemp)
            {
                _tempFiles.AddFile(temporaryFile, false);
            }

            File.WriteAllText(temporaryFile, source);

            var token = _cancellationTokenSource.Token;

            Action recompile = () => RecompileFile(temporaryFile, targetExec);

            Task.Run(
                () =>
                {
                    var fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(temporaryFile)) { Filter = Path.GetFileName(temporaryFile), NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName };

                    fileSystemWatcher.Changed += (sender, _) => recompile();
                    fileSystemWatcher.Renamed += (sender, _) => recompile();
                    fileSystemWatcher.Created += (sender, _) => recompile();

                    fileSystemWatcher.EnableRaisingEvents = true;

                    _log.Trace("Waiting for changes in " + temporaryFile);
                    while (!token.IsCancellationRequested)
                    {
                        fileSystemWatcher.WaitForChanged(WatcherChangeTypes.All, 500);
                    }
                }, token);

            if (wait)
            {
                _log.Trace("Press any key to quit");
                Console.Read();
            }
        }

        public static void RecompileFile(string sourceCodeFile, string targetExec)
        {
            _log.Trace("Recompiling " + sourceCodeFile + " to " + targetExec);
            var result = CompileCSharp.CompileFile(targetExec, sourceCodeFile);

            if (!result.Errors.HasErrors)
            {
                new AddSourceCode().Run(sourceCodeFile, targetExec);
            }
        }
    }
}