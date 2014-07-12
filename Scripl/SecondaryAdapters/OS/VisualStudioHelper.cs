using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Win32;

using Scripl.PortsOut;

namespace Scripl.SecondaryAdapters.OS
{
    public class VisualStudioHelper
    {        
        private readonly ITemporaryFileManager _temporaryFileManager;
        private readonly IUserSettings _settings;

        public VisualStudioHelper(ITemporaryFileManager temporaryFileManager, IUserSettings settings)
        {
            _temporaryFileManager = temporaryFileManager;
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
            File.WriteAllText(tempCSProjFile, csProjFileContent);

            var slnContent = _settings.slnTemplate
                .Replace("{SolutionGuid}", Guid.NewGuid().ToString())
                .Replace("{ProjectGuid}", Guid.NewGuid().ToString())
                .Replace("{projectName}", Path.GetFileNameWithoutExtension(tempCSProjFile))
                .Replace("{csprojPath}", tempCSProjFile);

            var slnFile = _temporaryFileManager.AddFileWithExtension("sln", keepFile: true);
            File.WriteAllText(slnFile, slnContent);

            var process = System.Diagnostics.Process.Start(slnFile);
            if (waitForEnd)
                process.WaitForExit();
        }
    }
}