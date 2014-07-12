using NDesk.Options;

using NLog;

using Scripl.PortsIn;

namespace Scripl.PrimaryAdapters.Console
{
    [Command("monitor")]
    [RunOnService]
    public class Monitor
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly IMonitor _monitor;

        public Monitor(IMonitor monitor)
        {
            _monitor = monitor;
        }

        public void Run(params string[] args)
        {
            bool wait = true;
            var notParsedArgs = (new OptionSet { { "no-wait", v => wait = v == null } }).Parse(args);
            
            var targetExec = notParsedArgs[1];
            var sourceCodeFile = notParsedArgs[0];

            _monitor.StartRecompilingOnChange(targetExec, sourceCodeFile);

            if (wait)
            {
                _log.Trace("Press any key to quit");
                System.Console.Read();
            }
        }
    }
}