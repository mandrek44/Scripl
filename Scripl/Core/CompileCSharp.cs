using System.CodeDom.Compiler;

using NLog;

using Scripl.Adapters.OS;
using Scripl.PortsIn;

namespace Scripl.Core
{
    internal class CompileCSharp : ICompileCSharp
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();
        private readonly Compiler _compiler = new Compiler();

        public void Run(string sourceFile, string targetFile)
        {
            var results = _compiler.CompileFile(targetFile, sourceFile);
            foreach (CompilerError error in results.Errors)
                _log.Trace("Line {0},{1}\t: {2}\r\n", error.Line, error.Column, error.ErrorText);
        }
    }
}