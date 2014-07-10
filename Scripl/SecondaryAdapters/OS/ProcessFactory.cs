using Scripl.PortsOut;

namespace Scripl.SecondaryAdapters.OS
{
    class ProcessFactory : IProcessFactory
    {
        public IProcess Start(string path)
        {
            return new Process(System.Diagnostics.Process.Start(path));
        }
    }
}