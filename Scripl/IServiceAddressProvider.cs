namespace Scripl.Commands
{
    public interface IServiceAddressProvider
    {
        string GetAddress();
    }

    class LocalhostAddressProvider : IServiceAddressProvider
    {
        public string GetAddress()
        {
            return string.Format("http://localhost:{0}", Properties.Settings.Default.Port);
        }
    }
}