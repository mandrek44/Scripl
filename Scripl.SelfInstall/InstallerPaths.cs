namespace Scripl.SelfInstall
{
    using System;
    using System.IO;
    using System.Reflection;

    public class InstallerPaths
    {
        private readonly string _company;
        private readonly string _product;

        public InstallerPaths(string companyName, string productName)
        {
            _company = companyName;
            _product = productName;

            ApplicationExecName = Path.GetFileName(Assembly.GetEntryAssembly().Location);
            RootDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), _company, _product);
            ProgramFilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), _company, _product);
            CurrentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            TargetApplicationExePath = Path.Combine(ProgramFilesPath, ApplicationExecName);
        }

        public string CurrentPath { get; private set; }

        public  string ProgramFilesPath { get; set; }

        public  string ApplicationExecName { get; set; }

        public string TargetApplicationExePath { get; set; }

        public string RootDataPath { get; set; }
    }
}