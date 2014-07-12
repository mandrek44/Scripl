namespace Scripl.PortsIn
{
    public interface IMonitor
    {
        void StartRecompilingOnChange(string targetExec, string sourceCodeFile);
    }
}