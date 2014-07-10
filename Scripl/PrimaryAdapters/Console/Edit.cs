using System.IO;

using NDesk.Options;

using NLog;

using Scripl.NotStructured;
using Scripl.PortsIn;

namespace Scripl.Adapters.Console
{
    [Command("edit")]
    public class Edit
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();
        private readonly IEdit _edit;

        public Edit(IEdit edit)
        {
            _edit = edit;
        }

        public void Run(params string[] args)
        {
            bool forceUseDefault = false;
            bool waitForUserInput = false;
            var unparsedArgs = (new OptionSet
                                {
                                    { "d|default", v=>forceUseDefault = v != null },
                                    { "w|wait", v=>waitForUserInput = v != null}
                                }).Parse(args);

            var exeName = Path.GetFullPath(unparsedArgs[0]);

            _edit.EditExec(exeName, forceUseDefault, waitForUserInput);

            if (waitForUserInput)
            {
                _log.Trace("Press any key to exit");
                System.Console.Read();
            }
        }
    }
}