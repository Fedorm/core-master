using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

using System.ServiceModel.Activation;
using System.Web.Routing;

namespace SyncOnPremises
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Common.Solution.InitContext(
                System.Configuration.ConfigurationManager.AppSettings["DataBaseServer"],
                System.Configuration.ConfigurationManager.AppSettings["SolutionsFolder"],
                System.Configuration.ConfigurationManager.AppSettings["BitMobileServerId"],
                false,
                System.Configuration.ConfigurationManager.AppSettings["RootPassword"]
            );

            //system end point
            RouteTable.Routes.Add(new ServiceRoute("system", new Microsoft.Synchronization.Services.SyncServiceHostFactoryEx("system"), typeof(AdminSync.SystemSyncService)));

            //solutions
            foreach (String dir in System.IO.Directory.EnumerateDirectories(System.Configuration.ConfigurationManager.AppSettings["SolutionsFolder"]))
            {
                String s = new System.IO.DirectoryInfo(dir).Name;
                RouteTable.Routes.Add(new ServiceRoute(String.Format("{0}/device", s), new Microsoft.Synchronization.Services.SyncServiceHostFactoryEx(s, true), typeof(Microsoft.Synchronization.Services.SyncServiceEx)));
                RouteTable.Routes.Add(new ServiceRoute(String.Format("{0}/admin", s), new Microsoft.Synchronization.Services.SyncServiceHostFactoryEx(s), typeof(AdminSync.AdminSyncService)));
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}