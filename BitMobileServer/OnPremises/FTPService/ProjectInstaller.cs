using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;


namespace FtpServer
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {
            if (!EventLog.SourceExists(Settings.SourceName))
            {
                EventLog.CreateEventSource(Settings.SourceName, "Application");
            }
        }

        private void serviceInstaller1_AfterUninstall(object sender, InstallEventArgs e)
        {
            if (EventLog.SourceExists(Settings.SourceName))
            {
                EventLog.DeleteEventSource(Settings.SourceName);
            }
        }
    }
}
