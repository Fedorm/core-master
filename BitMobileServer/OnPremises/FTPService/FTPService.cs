using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FtpServer
{
    public partial class FtpServer : ServiceBase
    {
        private static readonly int port = 21;

        public FtpServer()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            AddLog(String.Format("BitMobile FTP service started. Start listening on port {0}", port.ToString()));
            try
            {
                String[] arguments = Environment.GetCommandLineArgs();

                if (arguments.Length != 2)
                    throw new Exception(@"Web.config file should be provided as an argument. Modify registry entry ImagePath at HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\services\BitMobile FTP Service");

                Dictionary<String, String> settings = ReadSettings(arguments[1]);
                Common.Solution.InitContext(settings["DataBaseServer"], settings["SolutionsFolder"], settings["BitMobileServerId"], false, settings["RootPassword"]);

                String solutionFolder = settings["SolutionsFolder"];
                FtpService.FtpServer ftpServer = new FtpService.FtpServer(settings["SolutionsFolder"], port);

                ftpServer.Start();
            }
            catch (Exception e)
            {
                AddLog(e.Message, EventLogEntryType.Error);
                throw;
            }
        }

        protected override void OnStop()
        {
            AddLog("BitMobile FTP service stopped.");
        }

        private static Dictionary<String, String> ReadSettings(String configFile)
        {
            Dictionary<String, String> settings = new Dictionary<string, string>();

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(configFile);

            System.Xml.XmlNodeList nodes = doc.DocumentElement.SelectNodes(@"//configuration/appSettings/add");
            foreach (System.Xml.XmlNode node in nodes)
            {
                String key = node.Attributes["key"].Value;
                String value = node.Attributes["value"].Value;
                settings.Add(key, value);
            }

            return settings;
        }

        private void AddLog(String s, EventLogEntryType type = EventLogEntryType.Information)
        {
            try
            {
//                System.IO.File.AppendAllText(@"d:\log.txt", s + Environment.NewLine);

                EventLog.WriteEntry(Settings.SourceName, s, type);
            }
            catch (Exception)
            {
            }
        }
    }
}
