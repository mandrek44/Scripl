using Scripl.PortsIn;

namespace Scripl.PrimaryAdapters.Console
{
    [Command("add")]
    [RunOnService]
    internal class AddSourceCode
    {
        private readonly IAddSourceCode _addSourceCode;

        public AddSourceCode(IAddSourceCode addSourceCode)
        {
            _addSourceCode = addSourceCode;
        }

        public void Run(string sourceCodeFile, string targetExec)
        {
            _addSourceCode.Run(sourceCodeFile, targetExec);
        }
    }
}