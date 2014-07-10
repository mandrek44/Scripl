using System.IO;

using Scripl.Adapters;
using Scripl.PortsIn;
using Scripl.PortsOut;

namespace Scripl.Core
{
    public class New : INew
    {
        private readonly CommandRunner _commandRunner;

        private readonly ICompiler _compiler;
        private readonly ITemporaryFileManager _temporaryFiles;
        private readonly IAddSourceCode _addSourceCode;
        private readonly IEdit _edit;

        public New(CommandRunner commandRunner, ICompiler compiler, ITemporaryFileManager temporaryFiles, IAddSourceCode addSourceCode, IEdit edit)
        {
            _commandRunner = commandRunner;
            _compiler = compiler;
            _temporaryFiles = temporaryFiles;
            _addSourceCode = addSourceCode;
            _edit = edit;
        }

        public void Run()
        {
            var tempSourceFile = _temporaryFiles.AddFileWithExtension("cs");
            var source = Properties.Settings.Default.newScriplTemplate;
            var exeName = Path.GetFullPath("editme.exe");

            File.WriteAllText(tempSourceFile, source);

            _commandRunner.Invoke("compile", tempSourceFile, exeName);
            //_compiler.Compile(exeName, source);
            
            _commandRunner.Invoke("add", tempSourceFile, exeName);            
            // _addSourceCode.Run(tempSourceFile, exeName);

            _commandRunner.Invoke("edit", exeName);
            // _edit.EditExec(exeName, forceUseDefault: false, waitForUserInput: false);
        }
    }
}