using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;

using System.Net;
using System.Text.RegularExpressions;

namespace SyncRole
{
    public class SyncServiceWebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            ActivationContext.CreateActivationContext();

            StartFtp();

            return base.OnStart();
        }

        private void StartFtp()
        {
            if (!RoleEnvironment.IsEmulated)
            {
                InitContext();

                System.Net.IPEndPoint ep = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Ftp"].IPEndpoint;
                new FtpHelper(RoleEnvironment.GetLocalResource("Storage").RootPath, IPAddress(), ep.Port).Start();

            }
        }

        private void InitContext()
        {
            String dbServer = Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.GetConfigurationSettingValue("DatabaseServer");
            String solutionFolder = Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.GetLocalResource("Storage").RootPath;
            String rootPassword = Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.GetConfigurationSettingValue("RootPassword");
            
            Common.Solution.InitContext(dbServer, solutionFolder, RoleEnvironment.DeploymentId, !RoleEnvironment.IsEmulated, rootPassword);
        }

        private String IPAddress()
        {
            System.Net.WebClient client = new WebClient();
            try
            {
                using (System.IO.StreamReader r = new System.IO.StreamReader(client.OpenRead(@"http://ip-api.com/json")))
                {
                    String json = r.ReadToEnd();
                    //Regex regex = new System.Text.RegularExpressions.Regex(@"\""query\"":\""(?<ip>.+)\""");
                    Regex regex = new System.Text.RegularExpressions.Regex(@"\""query\"":\""(?<ip>[\d\.^\""]+)\""");
                    Match m = regex.Match(json);
                    if (m.Success)
                    {
                        return m.Groups["ip"].Value;
                    }
                }
            }
            catch
            {
            }
            return "";
        }
    }
}
