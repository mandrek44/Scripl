using System.CodeDom.Compiler;
using System.IO;

namespace Scripl.Commands
{
    [Command("new")]
    public class New
    {
        private readonly CommandRunner _runner;

        public New(CommandRunner runner)
        {
            _runner = runner;
        }

        public void Run()
        {
            using (var tempFileCollection = new TempFileCollection())
            {
                var tempSourceFile = tempFileCollection.AddExtension(".cs");
                
                var source = Properties.Settings.Default.newScriplTemplate;
                File.WriteAllText(tempSourceFile, source);

                var exeName = "editme.exe";

                CompileCSharp.Compile(exeName, source);
                _runner.Invoke("add", tempSourceFile, exeName);
            }
        }
    }
}