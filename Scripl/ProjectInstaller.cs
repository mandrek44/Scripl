﻿using System.ComponentModel;
using System.Configuration.Install;

namespace Scripl
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
