namespace Scripl.Ports
{
    public interface ISourceCodeRepository
    {
        SourceCode LoadSourceCode(string checksum);

        void SaveSourceCode(SourceCode sourceCode);
    }
}