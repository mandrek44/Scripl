namespace Scripl.PortsIn
{
    public interface IEdit
    {
        void EditExec(string exeName, bool forceUseDefault, bool waitForUserInput);
    }
}