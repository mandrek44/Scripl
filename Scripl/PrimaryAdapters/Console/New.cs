using System.IO;

using Scripl.PortsOut;

namespace Scripl.PrimaryAdapters.Console
{
    [Command("new")]
    public class New
    {
        private readonly CommandRunner _commandRunner;

        private readonly ITemporaryFileManager _temporaryFileManager;
        private readonly IUserSettings _userSettings;

        public New(CommandRunner commandRunner, ITemporaryFileManager temporaryFileManager, IUserSettings userSettings)
        {
            _commandRunner = commandRunner;
            _temporaryFileManager = temporaryFileManager;
            _userSettings = userSettings;
        }

        public void Run()
        {
            var tempSourceFile = _temporaryFileManager.AddFileWithExtension("cs");
            File.WriteAllText(tempSourceFile, _userSettings.newScriplTemplate);
            
            var exeName = Path.GetFullPath("editme.exe");

            _commandRunner.Invoke((CompileCSharp compile) => compile.Run(tempSourceFile, exeName));
            _commandRunner.Invoke((AddSourceCode add) => add.Run(tempSourceFile, exeName));
            _commandRunner.Invoke((Edit edit) => edit.Run(exeName));
        }
    }
}