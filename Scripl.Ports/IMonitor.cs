namespace Scripl.Ports
{
    public interface IMonitor
    {
        void StartRecompilingOnChange(string targetExec, string sourceCodeFile);
    }
}