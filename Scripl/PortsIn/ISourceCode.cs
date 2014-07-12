namespace Scripl.PortsIn
{
    public interface ISourceCode
    {
        string GetSourceCode(string exePath);

        void Save(string sourceCode, string targetExec);
    }
}