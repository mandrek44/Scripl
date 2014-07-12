using System.CodeDom.Compiler;

namespace Scritpl.Utils.Contracts
{
    public interface ICompiler
    {
        CompilerResults CompileFile(string tempExeName, string sourceFileName);

        CompilerResults Compile(string tempExeName, string sources);
    }
}