using Scripl.Data;

namespace Scripl.PortsOut
{
    public interface ISourceCodeRepository
    {
        SourceCode LoadSourceCode(string checksum);

        void SaveSourceCode(SourceCode sourceCode);
    }
}