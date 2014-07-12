namespace Scripl.Contracts
{
    public interface ISourceCodeRepository
    {
        SourceCode LoadSourceCode(string checksum);

        void SaveSourceCode(SourceCode sourceCode);
    }
}