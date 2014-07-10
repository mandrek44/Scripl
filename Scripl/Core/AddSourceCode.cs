using Scripl.Data;
using Scripl.NotStructured;
using Scripl.PortsIn;
using Scripl.PortsOut;

namespace Scripl.Core
{
    internal class AddSourceCode : IAddSourceCode
    {
        private readonly ISourceCodeRepository _repository;
        private readonly IFileSystem _fileSystem;

        public AddSourceCode(ISourceCodeRepository repository, IFileSystem fileSystem)
        {
            _repository = repository;
            _fileSystem = fileSystem;
        }

        public void Run(string sourceCodeFile, string targetExec)
        {
            _repository.SaveSourceCode(new SourceCode { Id = _fileSystem.GetChecksum(targetExec), Source = _fileSystem.ReadAllTextRetrying(sourceCodeFile) });
        }
    }

}