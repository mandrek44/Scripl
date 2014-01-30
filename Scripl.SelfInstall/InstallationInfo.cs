namespace Scripl.SelfInstall
{
    using System;
    using System.Reflection;

    public class InstallationInfo
    {
        public static InstallationInfo Default
        {
            get
            {
                var installationInfo = new InstallationInfo();
                var product = (AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyProductAttribute));
                if (product != null) 
                    installationInfo.ProgramName = product.Product;

                var company = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyCompanyAttribute));
                if (product != null)
                    installationInfo.Publisher = company.Company;

                installationInfo.Paths = new InstallerPaths(installationInfo.Publisher, installationInfo.ProgramName);
                installationInfo.UninstallCommand = string.Format("\"{0}\" uninstall", Assembly.GetEntryAssembly().Location);

                return installationInfo;
            }
        }

        public string ProgramName { get; set; }

        public string Id
        {
            get
            {
                return ProgramName.Replace(" ", string.Empty);
            }
        }

        public string Publisher { get; set; }

        public InstallerPaths Paths { get; set; }

        public string UninstallCommand { get; set; }
    }
}