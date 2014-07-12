using System.CodeDom.Compiler;

using NLog;

using Scripl.Attributes;

using Scritpl.Utils.Contracts;

namespace Scripl.Commands
{
    [Command("compile")]
    internal class CompileCSharp
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly ICompiler _compiler;

        public CompileCSharp(ICompiler compiler)
        {
            _compiler = compiler;
        }

        public void Run(string sourceFile, string targetFile)
        {
            var results = _compiler.CompileFile(targetFile, sourceFile);
            foreach (CompilerError error in results.Errors)
                _log.Trace("Line {0},{1}\t: {2}\r\n", error.Line, error.Column, error.ErrorText);
        }
    }
}