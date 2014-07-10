using Scripl.PortsOut;

namespace Scripl.SecondaryAdapters.OS
{
    class Process : IProcess
    {
        private System.Diagnostics.Process _process;

        public Process(System.Diagnostics.Process process)
        {
            _process = process;
        }

        public void WaitForExit()
        {
            _process.WaitForExit();
        }
    }
}