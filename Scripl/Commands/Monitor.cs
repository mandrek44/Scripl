using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Scripl.Data;

namespace Scripl.Commands
{
    [Command("monitor")]
    [RunOnService]
    public class Monitor
    {
        private CommandRunner _runner;

        private static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private static readonly TempFileCollection _tempFiles = new TempFileCollection();

        public Monitor(CommandRunner runner)
        {
            _runner = runner;
        }

        //public void Run(string sourceCodeFile, string targetExec)
        public void Run(params string[] args)
        {
            var targetExec = args[1];
            var sourceCodeFile = args[0];

            if (!File.Exists(targetExec))
            {
                Console.WriteLine(targetExec + " does not exists");
                throw new InvalidOperationException();
            }

            var checksum = FileHelper.GetChecksum(targetExec);

            SourceCode sourceCode = SourceCodeRepository.Instance.LoadSourceCode(checksum);

            if (sourceCode == null)
            {
                Console.WriteLine("Cannot find sources for " + targetExec);
                throw new FileNotFoundException();
            }

            var source = sourceCode.Source;
            var temporaryFile = sourceCodeFile;
            if (_runner.IsService)
            {
                _tempFiles.AddFile(temporaryFile, false);
            }

            File.WriteAllText(temporaryFile, source);

            var token = _cancellationTokenSource.Token;

            Action recompile = () => RecompileFile(temporaryFile, targetExec);

            Task.Run(
                () =>
                {
                    var fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(temporaryFile))
                                            { Filter = Path.GetFileName(temporaryFile), NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName };

                    fileSystemWatcher.Changed += (sender, _) => recompile();
                    fileSystemWatcher.Renamed += (sender, _) => recompile();
                    fileSystemWatcher.Created += (sender, _) => recompile();

                    fileSystemWatcher.EnableRaisingEvents = true;

                    Console.WriteLine("Waiting for changes in " + temporaryFile);
                    while (!token.IsCancellationRequested)
                    {
                        fileSystemWatcher.WaitForChanged(WatcherChangeTypes.All, 500);
                    }
                }, token);

            if (!_runner.IsService)
            {
                Console.WriteLine("Press any key to quit");
                Console.ReadKey();
            }
        }

        public static void RecompileFile(string sourceCodeFile, string targetExec)
        {
            Console.WriteLine("Recompiling " + sourceCodeFile + " to " + targetExec);
            var result = CompileCSharp.Compile(targetExec, sourceCodeFile);

            if (!result.Errors.HasErrors)
            {
                new AddSourceCode().Run(sourceCodeFile, targetExec);
            }
        }   
    }
}