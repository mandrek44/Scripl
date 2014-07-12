namespace Scripl.PortsOut
{
    public interface IUserSettings
    {
        string csprojTemplate { get; set; }

        string slnTemplate { get; set; }

        int Port { get; set; }

        string newScriplTemplate { get; set; }
    }
}