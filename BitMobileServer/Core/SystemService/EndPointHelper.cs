using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using System.ServiceModel.Activation;
using System.Reflection;

namespace SystemService
{
    public class EndPointHelper
    {
        public static void CreateAllEndPoints(String solutionFolder, BMWebDAV.FileSystem davRoot = null)
        {
            if (davRoot == null)
                CreateAllEndPointsInternal(solutionFolder, new BMWebDAV.DiskFileSystem(solutionFolder));
            else
                CreateAllEndPointsInternal(solutionFolder, davRoot);
        }

        private static void CreateAllEndPointsInternal(String solutionFolder, BMWebDAV.FileSystem davRoot)
        {
            //system end point
            CreateSystemEndPoint();

            //license end point
            CreateLicenseEndPoint();

            //webdav init
            BMWebDAV.BMWebDAVModule.Init(new BMWebDAV.DiskFileSystem(solutionFolder));

            //solutions
            foreach (String dir in System.IO.Directory.EnumerateDirectories(solutionFolder))
            {
                String s = new System.IO.DirectoryInfo(dir).Name;
                CreateEndPoints(s);
            }

            Common.Crypt.ReadLicenses();
        }

        public static void CreateEndPoints(String s)
        {
            CreateDeviceEndPoint(s);
            Common.Solution.Log(s, "admin", "device endpont created");

            CreateAdminEndPoint(s);
            Common.Solution.Log(s, "admin", "admin endpont created");

            CreateGPSEndPoint(s);
            Common.Solution.Log(s, "admin", "gps endpont created");

            CreateScriptEndPoint(s);
            Common.Solution.Log(s, "admin", "script endpont created");

            CreateWebDAVEndpoint(s);
            Common.Solution.Log(s, "admin", "webdav endpont created");

            CreatePushEndPoint(s);
            Common.Solution.Log(s, "admin", "push endpont created");
        }

        private static void CreateWebDAVEndpoint(String name)
        {
            BMWebDAV.BMWebDAVModule.AddSolution(name);
        }

        private static void CreateSystemEndPoint()
        {
            RouteTable.Routes.Add(new ServiceRoute("system", new Microsoft.Synchronization.Services.SyncServiceHostFactoryEx("system"), typeof(SystemService.SystemSyncService)));
        }

        private static void CreatePushEndPoint(String name)
        {
            RouteTable.Routes.Add(new ServiceRoute(String.Format("{0}/push", name), new Microsoft.Synchronization.Services.SyncServiceHostFactoryEx(name), typeof(PushService.PushService)));
        }

        private static void CreateAdminEndPoint(String name)
        {
            RouteTable.Routes.Add(new ServiceRoute(String.Format("{0}/admin", name), new Microsoft.Synchronization.Services.SyncServiceHostFactoryEx(name), typeof(AdminService.AdminSyncService)));
        }

        private static void CreateDeviceEndPoint(String name)
        {
            RouteTable.Routes.Add(new ServiceRoute(String.Format("{0}/device", name), new Microsoft.Synchronization.Services.SyncServiceHostFactoryEx(name, true), typeof(Microsoft.Synchronization.Services.SyncServiceEx)));
        }

        private static void CreateScriptEndPoint(String name)
        {
            RouteTable.Routes.Add(new ServiceRoute(String.Format("{0}/script", name), new Microsoft.Synchronization.Services.SyncServiceHostFactoryEx(name), typeof(ScriptService.ScriptService)));
        }

        private static void CreateGPSEndPoint(String name)
        {
            RouteTable.Routes.Add(new ServiceRoute(String.Format("{0}/gps", name), new Microsoft.Synchronization.Services.SyncServiceHostFactoryEx(name), typeof(GPSService.GPSService)));
        }

        private static void CreateFtpEndPoint(String solutionFolder)
        {
            FtpService.FtpServer ftpServer = new FtpService.FtpServer(solutionFolder, 21);
            ftpServer.Start();
        }

        private static bool CreateLicenseEndPoint()
        {
            Type t = null;
            try
            {
                Assembly a = System.Reflection.Assembly.LoadFile(String.Format(@"{0}\bin\LicenseService.dll", System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath));
                t = a.GetType("LicenseService.LicenseRequestHandler");
            }
            catch (Exception e)
            {
                return false;
            }

            if (t == null)
                return false;

            RouteTable.Routes.Add(new ServiceRoute("license", new Microsoft.Synchronization.Services.SyncServiceHostFactoryEx("license"), t));

            return true;
        }
    }
}
