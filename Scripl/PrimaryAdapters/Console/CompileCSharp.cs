using Scripl.PortsIn;

namespace Scripl.PrimaryAdapters.Console
{
    [Command("compile")]
    internal class CompileCSharp
    {
        private readonly ICSharpCompiler _icSharpCompiler;

        public CompileCSharp(ICSharpCompiler icSharpCompiler)
        {
            _icSharpCompiler = icSharpCompiler;
        }

        public void Run(string sourceFile, string targetFile)
        {
            _icSharpCompiler.CompileFile(sourceFile, targetFile);
        }
    }
}