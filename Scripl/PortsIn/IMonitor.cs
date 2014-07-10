namespace Scripl.PortsIn
{
    public interface IMonitor
    {
        void Run(string targetExec, string sourceCodeFile, bool isTemp);
    }
}