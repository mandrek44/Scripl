namespace Scripl.Contracts
{
    public interface IMonitor
    {
        void StartRecompilingOnChange(string targetExec, string sourceCodeFile);
    }
}