namespace Scripl.RecompilerService.Contract
{
    using ServiceStack.ServiceHost;

    public class EditScriplFile : IReturn<ScriplFileEditResult>
    {
        public string ExeFilePath { get; set; }

        public string TemporaryFile { get; set; }
    }
}
