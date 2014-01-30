namespace Scripl.RecompilerService.Contract
{
    public class ScriplFileEditResult
    {
        public static ScriplFileEditResult FileDoesntExist
        {
            get
            {
                return new ScriplFileEditResult { Status = EditScriplStatus.FileDoesntExist};
            }
        }

        public static ScriplFileEditResult NoSource
        {
            get
            {
                return new ScriplFileEditResult { Status = EditScriplStatus.NoSource };
            }
        }

        public string SourceFilePath { get; set; }
        public EditScriplStatus Status { get; set; }
    }
}