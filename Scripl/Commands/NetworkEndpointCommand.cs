using NLog;

using Scripl.Attributes;
using Scripl.Runtime;

namespace Scripl.Commands
{
    [Command("service")]
    internal class NetworkEndpointCommand
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly NetworkEndpoint _networkEndpoint;

        public NetworkEndpointCommand(NetworkEndpoint networkEndpoint)
        {
            _networkEndpoint = networkEndpoint;
        }

        public void Run()
        {
            using (_networkEndpoint.Start())
            {
                _log.Trace("Scripl host started");

                System.Console.ReadLine();

                _log.Trace("Scripl host exiting");
            }
        }
    }
}