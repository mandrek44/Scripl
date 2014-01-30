namespace Scripl.RecompilerService
{
    using System;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;

    using Scripl.SelfInstall;

    using Microsoft.CSharp;

    using NLog;

    using Scripl.RecompilerService.Contract;

    using ServiceStack.OrmLite;
    using ServiceStack.ServiceInterface;

    public class RecompilerService : Service
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly OrmLiteConnectionFactory _connectionFactory;
        private readonly TempFileCollection _tempFiles;

        internal static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public RecompilerService(OrmLiteConnectionFactory connectionFactory, TempFileCollection tempFiles, InstallerPaths paths)
        {
            _connectionFactory = connectionFactory;
            _tempFiles = tempFiles;
        }

        public object Post(EditScriplFile message)
        {
            if (!File.Exists(message.ExeFilePath))
            {
                return ScriplFileEditResult.FileDoesntExist;
            }

            var checksum = GetChecksum(message.ExeFilePath);

            ScriplHash scriplHash;
            using (var db = _connectionFactory.OpenDbConnection())
            {
                scriplHash = db.Select<ScriplHash>(s => s.Hash == checksum).FirstOrDefault();
            }

            if (scriplHash == null)
            {
                return ScriplFileEditResult.NoSource;
            }

            var source = scriplHash.Source;
            var temporaryFile = message.TemporaryFile;
            _tempFiles.AddFile(temporaryFile, false);
                
            File.WriteAllText(temporaryFile, source);

            var token = CancellationTokenSource.Token;

            Task.Run(
                () =>
                {
                    var fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(temporaryFile))
                                            {
                                                Filter = Path.GetFileName(temporaryFile),
                                                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName
                                            };

                    fileSystemWatcher.Changed += (sender, args) => OnChanged(message.ExeFilePath, temporaryFile);
                    fileSystemWatcher.Renamed += (sender, args) => OnChanged(message.ExeFilePath, temporaryFile);
                    fileSystemWatcher.Created += (sender, args) => OnChanged(message.ExeFilePath, temporaryFile);

                    fileSystemWatcher.EnableRaisingEvents = true;

                    while (!token.IsCancellationRequested)
                    {
                        Console.WriteLine("Waiting for changes in " + temporaryFile);
                        fileSystemWatcher.WaitForChanged(WatcherChangeTypes.All);
                    }
                });

            return new ScriplFileEditResult { SourceFilePath = temporaryFile };
        }

        public void Post(CreateScriplFile message)
        {
            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string initialFile = Path.Combine(currentPath, "initial.cs");

            Compile(message.ExecPath, initialFile);

            using (var db = _connectionFactory.OpenDbConnection())
            {
                db.Insert(new ScriplHash() { Hash = GetChecksum(message.ExecPath), Source = File.ReadAllText(initialFile) });
            }
        }

        private void OnChanged(string exeName, string temporaryFile)
        {
            _log.Trace("Recompiling " + temporaryFile + " to " + exeName);
            var result = Compile(exeName, temporaryFile);
            
            if (!result.Errors.HasErrors)
            {
                using (var db = _connectionFactory.OpenDbConnection())
                {
                    db.Insert(new ScriplHash() { Hash = GetChecksum(exeName), Source = SafeReadAllText(temporaryFile) });
                }
            }
        }

        private static string GetChecksum(string file)
        {
            using (var stream = new BufferedStream(File.OpenRead(file)))
            {
                byte[] checksum = new SHA256Managed().ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        private static CompilerResults Compile(string tempExeName, string sourceFileName)
        {
            var parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.GenerateExecutable = true;
            parameters.GenerateInMemory = false;
            
            parameters.OutputAssembly = tempExeName;
            
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Data.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
            parameters.ReferencedAssemblies.Add("System.Data.DataSetExtensions.dll");

            var provider = new CSharpCodeProvider();
            ICodeCompiler compiler = provider.CreateCompiler();
            CompilerResults results = compiler.CompileAssemblyFromSource(parameters, SafeReadAllText(sourceFileName));

            foreach (CompilerError error in results.Errors)
            {
                _log.Trace("Line {0},{1}\t: {2}\r\n",
                    error.Line, error.Column, error.ErrorText);
            }

            return results;
        }

        private static string SafeReadAllText(string sourceFileName)
        {
            for (int i = 0; i < 20; ++i)
            {
                try
                {
                    return File.ReadAllText(sourceFileName);
                }
                catch (IOException)
                {
                    if (i == 19) throw;

                    Console.WriteLine("Unable to read file, trying again...");
                    Thread.Sleep(500);
                }
            }

            return null;
        }
    }
}