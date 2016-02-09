using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;

using System.Net;
using System.Collections.Specialized;

namespace FtpRole
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            Dictionary<String, Func<NameValueCollection, String>> actions = new Dictionary<string, Func<NameValueCollection, string>>();
            actions.Add("start", StartFtpServer);
            IPEndPoint ep = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["CommandServer"].IPEndpoint;
            new CommandServer(ep.Address.ToString(), ep.Port, actions).Start();

            Thread.Sleep(Timeout.Infinite);
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;
            return base.OnStart();
        }

        private String StartFtpServer(System.Collections.Specialized.NameValueCollection parameters)
        {
            String root = Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.GetLocalResource("Storage").RootPath;

            //InitContext

            String[] solutions = parameters["solutions"].Split(';');
            foreach (String s in solutions)
            {
                String solutionFolder = String.Format(@"{0}\{1}", root, s);
                if (!System.IO.Directory.Exists(solutionFolder))
                    System.IO.Directory.CreateDirectory(solutionFolder);
            }

            FtpService.FtpServer ftpServer = new FtpService.FtpServer(root, 21);
            ftpServer.Start();

            return "ok";
        }

        public static void InitContext(String dbServer, String solutionFolder, String rootPassword)
        {
            Common.Solution.InitContext(dbServer, solutionFolder, RoleEnvironment.IsEmulated ? null : RoleEnvironment.DeploymentId, !RoleEnvironment.IsEmulated, rootPassword);
        }
    }
}
