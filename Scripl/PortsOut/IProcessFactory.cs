namespace Scripl.PortsOut
{
    public interface IProcessFactory
    {
        IProcess Start(string path);
    }
}