using Scripl.NotStructured;
using Scripl.PortsOut;

namespace Scripl.Adapters.OS
{
    class LocalhostAddressProvider : IServiceAddressProvider
    {
        private readonly IUserSettings _settings;

        public LocalhostAddressProvider(IUserSettings settings)
        {
            _settings = settings;
        }

        public string GetAddress()
        {
            return string.Format("http://localhost:{0}", _settings.Port);
        }
    }
}