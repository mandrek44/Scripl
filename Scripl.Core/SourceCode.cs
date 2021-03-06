using Scripl.Ports;
using Scripl.Utils.Contracts;

namespace Scripl.Core
{
    internal class SourceCode : ISourceCode
    {
        private readonly ISourceCodeRepository _repository;
        private readonly IFileSystem _fileSystem;

        public SourceCode(ISourceCodeRepository repository, IFileSystem fileSystem)
        {
            _repository = repository;
            _fileSystem = fileSystem;
        }

        public void Save(string sourceCode, string targetExec)
        {
            _repository.SaveSourceCode(new Ports.SourceCode { Id = _fileSystem.GetChecksum(targetExec), Source = sourceCode });
        }

        public string GetSourceCode(string exePath)
        {
            return _repository.LoadSourceCode(_fileSystem.GetChecksum(exePath)).Source;
        }
    }

}