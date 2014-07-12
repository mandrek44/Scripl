using System.ComponentModel;
using System.Configuration.Install;

namespace Scripl.Runtime
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
