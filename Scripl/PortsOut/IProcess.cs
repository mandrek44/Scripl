using System.Diagnostics;

namespace Scripl.PortsOut
{
    public interface IProcess
    {
        void WaitForExit();
    }
}