using Scripl.NotStructured;
using Scripl.PortsIn;

namespace Scripl.Adapters.Console
{
    [Command("compile")]
    internal class CompileCSharp
    {
        private readonly ICompileCSharp _compileCSharp;

        public CompileCSharp(ICompileCSharp compileCSharp)
        {
            _compileCSharp = compileCSharp;
        }

        public void Run(string sourceFile, string targetFile)
        {
            _compileCSharp.Run(sourceFile, targetFile);
        }
    }
}