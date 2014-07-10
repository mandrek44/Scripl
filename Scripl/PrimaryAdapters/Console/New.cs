using Scripl.NotStructured;
using Scripl.NotStructured.Commands;
using Scripl.PortsIn;

namespace Scripl.Adapters.Console
{
    [Command("new")]
    public class New
    {
        private readonly INew _newCommand;

        public New(INew newCommand)
        {
            _newCommand = newCommand;
        }

        public void Run()
        {
            _newCommand.Run();
        }
    }
}