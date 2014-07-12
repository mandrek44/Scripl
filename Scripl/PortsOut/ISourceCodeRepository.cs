using Scripl.Contracts;

namespace Scripl.PortsOut
{
    public interface ISourceCodeRepository
    {
        SourceCode LoadSourceCode(string checksum);

        void SaveSourceCode(SourceCode sourceCode);
    }
}