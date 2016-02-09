using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;

namespace Common
{
    [Serializable]
    public class Solution
    {
        private static object threadlock;
        private static String _dbServer;        
        private static String _rootFolder;
        private static String _bitMobileServerId;
        private static bool _isAzured;
        private static String _rootPassword;

        public static String HostKillerName(string scope, String mode)
        {
            return String.Format("KillServiceHost_{0}_{1}", scope, mode);
        }

        public String DatabaseServer 
        {
            get
            {
                return _dbServer;
            }
        }
        
        public String RootFolder 
        {
            get
            {
                return _rootFolder;
            }
        }

        public String RootPassword
        {
            get
            {
                return _rootPassword;
            }
        }

        public String SolutionPassword
        {
            get
            {
                try
                {
                    return System.IO.File.ReadAllText(String.Format(@"{0}\access\password.txt", SolutionFolder));
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        public String SettingsFile
        {
            get
            {
                return String.Format(@"{0}\resource\settings.xml", SolutionFolder);
            }
        }

        public static String GetSolutionFolder(String scope)
        {
            return System.IO.Path.Combine(_rootFolder, scope);
        }

        public bool IsAsured
        {
            get
            {
                return _isAzured;
            }
        }

        public String BitMobileServerId
        {
            get
            {
                return _bitMobileServerId;
            }
        }

        public String Name { get; set; }

        public String ConnectionString
        {
            get
            {
                return String.Format("{0};Initial catalog={1}", DatabaseServer, DatabaseName);
            }
        }

        public String DatabaseName
        {
            get
            {
                return String.Format("BitMobile_{0}{1}", _bitMobileServerId == null ? "" : _bitMobileServerId + "_", Name);
            }
        }

        public String SolutionFolder
        {
            get
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(_rootFolder, Name));
                return System.IO.Path.Combine(_rootFolder, Name);
            }
        }

        public String DeviceResourceFolder
        {
            get
            {
                if (System.IO.Directory.Exists(System.IO.Path.Combine(SolutionFolder, @"resource\device")))
                    return System.IO.Path.Combine(SolutionFolder, @"resource\device");
                return System.IO.Path.Combine(SolutionFolder, "resource");
            }
        }

        public bool ResourcesExists
        {
            get
            {
                String folder = DeviceResourceFolder;
                if (!System.IO.Directory.Exists(folder))
                    return false;
                return (System.IO.Directory.GetDirectories(folder).Length + System.IO.Directory.GetFiles(folder).Length) > 0;
            }
        }

        public String ConfigurationFile
        {
            get
            {
                return String.Format(@"{0}\configuration\configuration.xml", SolutionFolder);
            }
        }

        public static void InitContext(String databaseServer, String rootFolder, String bitMobileServerId, bool isAzured, String rootPassword)
        {
            threadlock = new object();
            _dbServer = databaseServer;
            _rootFolder = rootFolder;
            _bitMobileServerId = bitMobileServerId;
            _isAzured = isAzured;
            _rootPassword = rootPassword;
        }

        public static Solution CreateFromContext(string scope)
        {
            Solution info = new Solution();
            info.Name = scope;
            return info;
        }


        public static String LogFile(String scope, String type)
        {
            try
            {
                String dir = System.IO.Path.Combine(GetSolutionFolder(scope), "log");
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
                return String.Format(@"{0}\{1}.txt", dir, type);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static void Log(String scope, String type, String message)
        {
            try
            {
                String logFile = LogFile(scope, type);
                if (!String.IsNullOrEmpty(logFile))
                {
                    lock (threadlock)
                    {
                        System.IO.File.AppendAllText(logFile, String.Format("{0} {1}: {2}{3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), message, System.Environment.NewLine));
                    }
                }
            }
            catch(Exception)
            {
            }
        }

        public static void LogException(String scope, String type, Exception e)
        {
            try
            {
                String text = e.Message;
                while (e.InnerException != null)
                {
                    text = text + "; " + e.InnerException.Message;
                    e = e.InnerException;
                }
                text = text + e.StackTrace;

                Log(scope, type, text);
            }
            catch (Exception)
            {
            }
        }

        public SolutionStaticBox staticBox;

        [System.Runtime.Serialization.OnSerializing]
        public void OnSerializing(System.Runtime.Serialization.StreamingContext context)
        {
            staticBox = new SolutionStaticBox();
            staticBox._bitMobileServerId = _bitMobileServerId;
            staticBox._rootFolder = _rootFolder;
            staticBox._dbServer = _dbServer;
            staticBox._isAzured = _isAzured;
            staticBox._rootPassword = _rootPassword;
        }

        [System.Runtime.Serialization.OnDeserialized]
        public void OnDeserialized(System.Runtime.Serialization.StreamingContext context)
        {
            if(staticBox!=null)
            {
                _rootFolder = staticBox._rootFolder;
                _bitMobileServerId = staticBox._bitMobileServerId;
                _dbServer = staticBox._dbServer;
                _isAzured = staticBox._isAzured;
                _rootPassword = staticBox._rootPassword;
            }
        }


        public static void ChangeHostState(String scope, String state)
        {
            try
            {
                System.Threading.EventWaitHandle evt = System.Threading.EventWaitHandle.OpenExisting(Solution.HostKillerName(scope, state));
                evt.Set();
            }
            catch (Exception)
            {
            }
        }

    }

    [Serializable]
    public class SolutionStaticBox
    {
        public String _dbServer;
        public String _rootFolder;
        public String _bitMobileServerId;
        public bool _isAzured;
        public String _rootPassword;
    }
}
