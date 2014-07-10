using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Win32;

using Scripl.PortsOut;

namespace Scripl.Adapters.OS
{
    public class VisualStudioHelper : IVisualStudioHelper
    {
        private readonly IFileSystem _fileSystem;
        private readonly ITemporaryFileManager _temporaryFileManager;
        private readonly IProcessFactory _processFactory;
        private readonly IUserSettings _settings;

        public VisualStudioHelper(IFileSystem fileSystem, ITemporaryFileManager temporaryFileManager, IProcessFactory processFactory, IUserSettings settings)
        {
            _fileSystem = fileSystem;
            _temporaryFileManager = temporaryFileManager;
            _processFactory = processFactory;
            _settings = settings;
        }

        public bool IsInstalled()
        {
            var regex = new Regex(@"^VisualStudio\.edmx\.(\d+)\.(\d+)$");
            return Registry.ClassesRoot.GetSubKeyNames().Any(regex.IsMatch);
        }

        public void EditSourceFile(string sourceFile, bool waitForEnd)
        {
            var csProjFileContent = _settings.csprojTemplate.Replace("{sourceFileName}", Path.GetFileName(sourceFile));
            var tempCSProjFile = _temporaryFileManager.AddFileWithExtension("csproj", keepFile: true);
            _fileSystem.WriteAllText(tempCSProjFile, csProjFileContent);

            var slnContent = _settings.slnTemplate
                .Replace("{SolutionGuid}", Guid.NewGuid().ToString())
                .Replace("{ProjectGuid}", Guid.NewGuid().ToString())
                .Replace("{projectName}", Path.GetFileNameWithoutExtension(tempCSProjFile))
                .Replace("{csprojPath}", tempCSProjFile);

            var slnFile = _temporaryFileManager.AddFileWithExtension("sln", keepFile: true);
            _fileSystem.WriteAllText(slnFile, slnContent);

            var process = _processFactory.Start(slnFile);
            if (waitForEnd)
                process.WaitForExit();
        }
    }
}