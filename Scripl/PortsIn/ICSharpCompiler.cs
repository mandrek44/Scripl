namespace Scripl.PortsIn
{
    internal interface ICSharpCompiler
    {
        void CompileFile(string sourceFile, string targetFile);
    }
}