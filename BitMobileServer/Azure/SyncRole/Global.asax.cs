using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Role
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            InitContext();
        }

        public void InitContext()
        {
            String dbServer;
            if (RoleEnvironment.IsEmulated)
                dbServer = System.Configuration.ConfigurationManager.AppSettings["DataBaseServer"];
            else
                dbServer = Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.GetConfigurationSettingValue("DatabaseServer");

            String solutionFolder;
            if (RoleEnvironment.IsEmulated)
                solutionFolder = System.Configuration.ConfigurationManager.AppSettings["SolutionsFolder"];
            else
            {
                solutionFolder = Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.GetLocalResource("Storage").RootPath;
            }

            String rootPassword;
            if (RoleEnvironment.IsEmulated)
                rootPassword = System.Configuration.ConfigurationManager.AppSettings["RootPassword"];
            else
            {
                rootPassword = Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.GetConfigurationSettingValue("RootPassword");
            }

            Common.Solution.InitContext(dbServer, solutionFolder, RoleEnvironment.IsEmulated ? null : RoleEnvironment.DeploymentId, !RoleEnvironment.IsEmulated, rootPassword);

            String storageConnectionString = null;
            try
            {
                storageConnectionString = Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.GetConfigurationSettingValue("BlobStorage");                
            }
            catch
            {
            }

            /*
            if (storageConnectionString != null)
            {
                BMWebDAV.AzureFileSystem fs = new BMWebDAV.AzureFileSystem(storageConnectionString, RoleEnvironment.DeploymentId);
                SystemService.EndPointHelper.CreateAllEndPoints(solutionFolder, fs);
            }
            else
                SystemService.EndPointHelper.CreateAllEndPoints(solutionFolder); //use local file system
            */
            SystemService.EndPointHelper.CreateAllEndPoints(solutionFolder); //use local file system
        }
    }
}