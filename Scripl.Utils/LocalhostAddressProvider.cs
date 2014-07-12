using Scripl.Utils.Contracts;

namespace Scripl.Utils
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