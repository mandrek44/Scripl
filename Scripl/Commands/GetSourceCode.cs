using Scripl.Attributes;
using Scripl.Contracts;

namespace Scripl.Commands
{
    [Command("get")]
    [RunOnService]
    public class GetSourceCode
    {
        private readonly ISourceCode _sourceCode;

        public GetSourceCode(ISourceCode sourceCode)
        {
            _sourceCode = sourceCode;
        }

        public string Run(string exePath)
        {
            return _sourceCode.GetSourceCode(exePath);
        }
    }
}