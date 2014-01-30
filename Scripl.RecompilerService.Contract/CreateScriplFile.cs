namespace Scripl.RecompilerService.Contract
{
    using ServiceStack.ServiceHost;

    public class CreateScriplFile : IReturnVoid
    {
        public string ExecPath { get; set; }
    }
}