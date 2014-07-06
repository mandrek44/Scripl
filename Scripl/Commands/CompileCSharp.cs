using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Threading;

using Microsoft.CSharp;

namespace Scripl.Commands
{
    [Command("compile")]
    internal class CompileCSharp
    {
        public void Run(string sourceFile, string targetFile)
        {
            Compile(targetFile, sourceFile);
        }

        public static CompilerResults Compile(string tempExeName, string sourceFileName)
        {
            var parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.GenerateExecutable = true;
            parameters.GenerateInMemory = false;

            parameters.OutputAssembly = tempExeName;

            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Data.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
            parameters.ReferencedAssemblies.Add("System.Data.DataSetExtensions.dll");

            var compiler = new CSharpCodeProvider().CreateCompiler();
            var results = compiler.CompileAssemblyFromSource(parameters, SafeReadAllText(sourceFileName));

            foreach (CompilerError error in results.Errors)
                Console.WriteLine("Line {0},{1}\t: {2}\r\n", error.Line, error.Column, error.ErrorText);

            return results;
        }

        private static string SafeReadAllText(string sourceFileName)
        {
            for (int i = 0; i < 20; ++i)
            {
                try
                {
                    return File.ReadAllText(sourceFileName);
                }
                catch (IOException)
                {
                    if (i == 19) throw;

                    Thread.Sleep(500);
                }
            }

            return null;
        }
    }
}