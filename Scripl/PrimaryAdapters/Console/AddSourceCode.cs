using Scripl.PortsIn;
using Scripl.PortsOut;

namespace Scripl.PrimaryAdapters.Console
{
    [Command("add")]
    [RunOnService]
    internal class AddSourceCode
    {
        private readonly ISourceCode _sourceCode;
        private readonly IFileSystem _fileSystem;

        public AddSourceCode(ISourceCode sourceCode, IFileSystem fileSystem)
        {
            _sourceCode = sourceCode;
            _fileSystem = fileSystem;
        }

        public void Run(string sourceCodeFile, string targetExec)
        {
            _sourceCode.Save(_fileSystem.ReadAllTextRetrying(sourceCodeFile), targetExec);
        }
    }
}