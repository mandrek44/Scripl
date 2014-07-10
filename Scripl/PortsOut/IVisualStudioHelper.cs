namespace Scripl.PortsOut
{
    public interface IVisualStudioHelper
    {
        bool IsInstalled();

        void EditSourceFile(string sourceFile, bool waitForEnd);
    }
}