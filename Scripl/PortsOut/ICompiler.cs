using System.CodeDom.Compiler;

namespace Scripl.PortsOut
{
    public interface ICompiler
    {
        CompilerResults CompileFile(string tempExeName, string sourceFileName);

        CompilerResults Compile(string tempExeName, string sources);
    }
}