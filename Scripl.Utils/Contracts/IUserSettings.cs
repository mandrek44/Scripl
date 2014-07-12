namespace Scripl.Utils.Contracts
{
    public interface IUserSettings
    {
        string csprojTemplate { get; set; }

        string slnTemplate { get; set; }

        int Port { get; set; }

        string newScriplTemplate { get; set; }

        string DataDirectory { get; set; }
    }
}