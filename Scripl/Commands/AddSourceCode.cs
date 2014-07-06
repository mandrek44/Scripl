using Scripl.Data;

namespace Scripl.Commands
{
    [Command("add")]
    [RunOnService]
    internal class AddSourceCode
    {
        public void Run(string sourceCodeFile, string targetExec)
        {
            SourceCodeRepository.Instance.SaveSourceCode(new SourceCode { Id = FileHelper.GetChecksum(targetExec), Source = FileHelper.SafeReadAllText(sourceCodeFile) });
        }
    }
}