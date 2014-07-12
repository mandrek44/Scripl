using Scripl.PortsOut;

namespace Scripl.SecondaryAdapters.OS
{
    class UserSettings : IUserSettings
    {
        private readonly Properties.Settings _settings = Properties.Settings.Default;

        public string csprojTemplate
        {
            get
            {
                return _settings.csprojTemplate;
            }
            set
            {
                _settings.csprojTemplate = value;
            }
        }

        public int Port
        {
            get
            {
                return _settings.Port;
            }
            set
            {
                _settings.Port = value;
            }
        }

        public string newScriplTemplate
        {
            get
            {
                return _settings.newScriplTemplate;
            }
            set
            {
                _settings.newScriplTemplate = value;
            }
        }

        public string slnTemplate
        {
            get
            {
                return _settings.slnTemplate;
            }
            set
            {
                _settings.slnTemplate = value;
            }
        }
    }
}