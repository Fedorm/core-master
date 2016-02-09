using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPServer
{
    class Program
    {
        private static readonly int port = 21;

        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 1)
                    throw new Exception(@"Web.config file should be provided as an argument.");

                Dictionary<String, String> settings = ReadSettings(args[0]);

                Common.Solution.InitContext(settings["DataBaseServer"], settings["SolutionsFolder"], settings["BitMobileServerId"], false, settings["RootPassword"]);

                FtpService.FtpServer ftpServer = new FtpService.FtpServer(settings["SolutionsFolder"], port);
                ftpServer.Start();
                Console.WriteLine(String.Format("BitMobile FTP server started. Start listening on port {0}", port.ToString()));
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
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
    }
}
