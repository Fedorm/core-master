using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemService
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

            SystemService.EndPointHelper.CreateAllEndPoints(System.Configuration.ConfigurationManager.AppSettings["SolutionsFolder"]);
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
