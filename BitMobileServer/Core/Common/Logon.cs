using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace Common
{
    public static class Logon
    {
        private static object devicesSyncObj = new object();

        public static void CheckRootCredential(String scope, System.Net.NetworkCredential credential)
        {
            if (credential != null)
            {
                if (credential.UserName.ToLower().Equals("admin"))
                {
                    if (credential.Password.Equals(Common.Solution.CreateFromContext(scope).RootPassword))
                        return;
                }
            }
            throw new UnauthorizedAccessException();
        }

        public static void CheckAdminCredential(String scope, System.Net.NetworkCredential credential)
        {
            String pwd = Common.Solution.CreateFromContext(scope).SolutionPassword;
            if (!String.IsNullOrEmpty(pwd))
            {
                if (credential != null)
                {
                    if (credential.UserName.ToLower().Equals("admin") && credential.Password.Equals(pwd))
                        return;
                }
                throw new UnauthorizedAccessException();
            }
            else
                throw new UnauthorizedAccessException("Solution password is not set");
        }

        public static void CheckUserCredential(String scope, System.Net.NetworkCredential credential)
        {
            Guid userId;
            if (System.Guid.TryParse(credential.UserName, out userId))
            {
                if (!Common.Logon.UserExists(scope, userId, credential.Password))
                    throw new UnauthorizedAccessException();
            }
            else
                throw new UnauthorizedAccessException();
        }

        public static void CheckUserCredential2(String scope, System.Net.NetworkCredential credential)
        {
            if (!Common.Logon.UserExists2(scope, credential.UserName, credential.Password))
                throw new UnauthorizedAccessException();
        }

        public static void CheckVersion(String coreVersion, String configVersion, String type, int mode = 0)
        {
            try
            {
                Version ver = new Version(configVersion);

                if (String.IsNullOrEmpty(coreVersion))
                    throw new UnsupportedCoreException(String.Format("Unsupported {0}. Version '{1}' or greater is required", type, configVersion));

                Version cv = new Version(coreVersion);

                if (mode == 0)
                {
                    if (ver.CompareTo(cv) > 0)
                        throw new UnsupportedCoreException(String.Format("Unsupported {0} '{1}'. Version '{2}' or greater is required", type, coreVersion, configVersion));

                    if (cv.Major != ver.Major)
                        throw new UnsupportedCoreException(String.Format("Unsupported {0} '{1}'. Major version is incompatible with server version '{2}'", type, coreVersion, configVersion));
                }

                if (mode == 1)
                {
                    if (!(ver.Major.Equals(cv.Major) && ver.Minor.Equals(cv.Minor) && ver.Build.Equals(cv.Build)))
                        throw new UnsupportedCoreException(String.Format("Unsupported {0} '{1}'. Version '{2}' is required", type, coreVersion, configVersion));
                }

            }
            catch (FormatException)
            {
                throw new System.FormatException(String.Format("Invalid {0} version format", type));
            }

        }


        public static void CheckSupportedMetaVersion(String scope, String coreVersion)
        {
            String supportedMeta = Common.Logon.GetSupportedMetaVersion(scope);
            if (!String.IsNullOrEmpty(supportedMeta))
            {
                CheckVersion(coreVersion, supportedMeta, "meta", 1);
            }
        }

        public static void CheckCoreVersion(String scope, String coreVersion)
        {
            String settingsFile = Solution.CreateFromContext(scope).SettingsFile;
            if (System.IO.File.Exists(settingsFile))
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load(settingsFile);
                System.Xml.XmlNode node = doc.DocumentElement.SelectSingleNode(@"client/supportedCore");
                if (node != null)
                {
                    System.Xml.XmlAttribute attr = node.Attributes["version"];
                    if (attr != null)
                    {
                        String configVersion = attr.Value;
                        CheckVersion(coreVersion, configVersion, "core");
                    }
                }
            }
        }

        public static String GetResourceVersion(String scope)
        {
            String settingsFile = Solution.CreateFromContext(scope).SettingsFile;
            if (System.IO.File.Exists(settingsFile))
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load(settingsFile);
                System.Xml.XmlNode node = doc.DocumentElement.SelectSingleNode(@"resource/version");
                if (node != null)
                {
                    System.Xml.XmlAttribute attr = node.Attributes["version"];
                    if (attr != null)
                    {
                        return attr.Value;
                    }
                }
            }
            return null;
        }

        public static String GetSupportedMetaVersion(String scope)
        {
            String settingsFile = Solution.CreateFromContext(scope).SettingsFile;
            if (System.IO.File.Exists(settingsFile))
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load(settingsFile);
                System.Xml.XmlNode node = doc.DocumentElement.SelectSingleNode(@"server/supportedMeta");
                if (node != null)
                {
                    System.Xml.XmlAttribute attr = node.Attributes["version"];
                    if (attr != null)
                    {
                        return attr.Value;
                    }
                }
            }
            return null;
        }

        public static bool CheckIfFilterShared(String scope)
        {
            String settingsFile = Solution.CreateFromContext(scope).SettingsFile;
            if (System.IO.File.Exists(settingsFile))
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load(settingsFile);
                System.Xml.XmlNode node = doc.DocumentElement.SelectSingleNode(@"server/filterShared");
                if (node != null)
                {
                    System.Xml.XmlAttribute attr = node.Attributes["value"];
                    if (attr != null)
                    {
                        if (attr.Value == null)
                            return false;
                        else
                            return attr.Value.ToLower().Equals("yes");
                    }
                }
            }
            return false;
        }

        public static String GetConflictPolicy(String scope)
        {
            String settingsFile = Solution.CreateFromContext(scope).SettingsFile;
            if (System.IO.File.Exists(settingsFile))
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load(settingsFile);
                System.Xml.XmlNode node = doc.DocumentElement.SelectSingleNode(@"server/conflictPolicy");
                if (node != null)
                {
                    System.Xml.XmlAttribute attr = node.Attributes["value"];
                    if (attr != null)
                        return attr.Value;
                }
            }
            return null;
        }

        private static ConcurrentDictionary<String, DateTime> devices = new ConcurrentDictionary<string, DateTime>();

        public static bool CheckServerLicence(String deviceId)
        {
            Trace.TraceInformation("Checking server license for device {0}", deviceId);

            Crypt.ReadLicenses(); //refresh

            var licences = Crypt.GetLicenses();
            if (!licences.Any(l => (l.ExpireDate - DateTime.Now).Days >= 0))
            {
                Trace.TraceInformation("All licenses expired!");
                return false;
            }

            devices.AddOrUpdate(deviceId, DateTime.Now, (key, oldValue) => DateTime.Now);
            int cnt = 0;
            foreach (DateTime v in devices.Values)
            {
                if ((DateTime.Now - v).Duration().Days == 0)
                    cnt++;
            }

            Trace.TraceInformation("Devices: {0}, total licenses: {1}", cnt, Crypt.TotalLicenses);
            Trace.TraceInformation("CheckServerLicence returned {0}", cnt <= Crypt.TotalLicenses);
            return cnt <= Crypt.TotalLicenses;
        }

        public static void CheckLicense(String scope, String deviceId)
        {
            if (String.IsNullOrEmpty(deviceId)) //unbelievable, but..
            {
                Trace.TraceWarning("Empty device id!");
                return;
            }

            lock (devicesSyncObj)
            {
                Trace.TraceInformation("Checking license for device {0}", deviceId);

                String path = System.IO.Path.Combine(Common.Solution.GetSolutionFolder(scope), "access");
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(path);

                String licensesPath = Common.Crypt.GetLicensesPath();
                int licenses = 0; //unlim
                if (System.IO.File.Exists(licensesPath))
                    int.TryParse(System.IO.File.ReadAllText(licensesPath), out licenses);

                Trace.TraceInformation("Max licenses: {0}", licenses);

                String devicesPath = String.Format(@"{0}\devices.txt", path);
                List<String> ids = new List<string>();
                bool flag = false;

                if (System.IO.File.Exists(devicesPath))
                {
                    Trace.TraceInformation("Processing {0}", path);

                    var lines = System.IO.File.ReadLines(devicesPath);
                    foreach (String line in lines)
                    {
                        String s = line.Trim();
                        if (!String.IsNullOrEmpty(s))
                        {
                            String[] arr = s.Split(';'); //id
                            if (arr.Length == 2)
                            {
                                DateTime dt = DateTime.Parse(arr[1]); //last access
                                if ((DateTime.Now - dt).Duration().Days == 0)
                                {
                                    if (arr[0].Equals(deviceId))
                                    {
                                        flag = true;
                                        s = String.Format("{0};{1}", arr[0], DateTime.Now.ToString()); //update date
                                        Trace.TraceInformation("Found device {0} in devices.txt", deviceId);
                                    }
                                    ids.Add(s);
                                }
                            }
                        }
                    }
                }

                if (!flag) //deviceid not in list
                {
                    Trace.TraceInformation("Not found device {0} in devices.txt. Total devices: {1}, total licenses: {2}", deviceId, ids.Count, licenses);
                    if (ids.Count >= licenses && licenses != 0)
                        throw new LicenseException("Limit of licenses exceeded");
                    else
                    {
                        Trace.TraceInformation("Adding device {0} to devices.txt", deviceId);
                        ids.Add(String.Format("{0};{1}", deviceId, DateTime.Now.ToString()));
                    }
                }

                System.IO.File.WriteAllLines(devicesPath, ids);
            }
        }

        public static Guid GetUserId(String databaseName, System.Net.NetworkCredential credential, String configName, String configVersion)
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(Common.Solution.CreateFromContext(databaseName).ConnectionString))
            {
                conn.Open();
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("[admin].[Logon]", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                System.Data.SqlClient.SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters["@UserName"].Value = credential.UserName;
                cmd.Parameters["@UserPassword"].Value = credential.Password;
                cmd.Parameters["@ConfigName"].Value = configName;
                cmd.Parameters["@ConfigVersion"].Value = configVersion;
                try
                {
                    using (System.Data.SqlClient.SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            return r.GetGuid(0);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new ConflictVersionException(e.Message);
                }
            }
            throw new UnauthorizedAccessException("Invalid user name or password");
        }

        public static Guid GetUserId2(String databaseName, System.Net.NetworkCredential credential, String configName, String configVersion, out String eMail)
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(Common.Solution.CreateFromContext(databaseName).ConnectionString))
            {
                conn.Open();
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("[admin].[Logon]", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                System.Data.SqlClient.SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters["@UserName"].Value = credential.UserName;
                cmd.Parameters["@UserPassword"].Value = credential.Password;
                cmd.Parameters["@ConfigName"].Value = configName;
                cmd.Parameters["@ConfigVersion"].Value = configVersion;
                try
                {
                    using (System.Data.SqlClient.SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            eMail = r.GetString(1);
                            return r.GetGuid(0);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new ConflictVersionException(e.Message);
                }
            }
            throw new UnauthorizedAccessException("Invalid user name or password");
        }

        public static bool UserExists(String databaseName, Guid userId, String password)
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(Common.Solution.CreateFromContext(databaseName).ConnectionString))
            {
                conn.Open();
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("[admin].[CheckUser]", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                System.Data.SqlClient.SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters["@UserId"].Value = userId;
                cmd.Parameters["@UserPassword"].Value = password;
                try
                {
                    using (System.Data.SqlClient.SqlDataReader r = cmd.ExecuteReader())
                    {
                        return r.HasRows;
                    }
                }
                catch (Exception e)
                {
                    throw new UnauthorizedAccessException(e.Message);
                }
            }
        }

        public static bool UserExists2(string databaseName, string userName, string password)
        {
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(Common.Solution.CreateFromContext(databaseName).ConnectionString))
            {
                conn.Open();
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand("SELECT TOP 1 Id FROM [Catalog].[User] WHERE UserName=@UserName AND Password=@UserPassword", conn);
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@UserName", userName);
                cmd.Parameters.AddWithValue("@UserPassword", password);
                try
                {
                    using (System.Data.SqlClient.SqlDataReader r = cmd.ExecuteReader())
                    {
                        return r.HasRows;
                    }
                }
                catch (Exception e)
                {
                    throw new UnauthorizedAccessException(e.Message);
                }
            }
        }

        public static void WriteLog(String databaseName, Dictionary<String, object> dict)
        {
            using (var conn = new SqlConnection(Solution.CreateFromContext(databaseName).ConnectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("[admin].[WriteLog]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlCommandBuilder.DeriveParameters(cmd);

                for (int i = 1; i < cmd.Parameters.Count; i++)
                {
                    String pName = cmd.Parameters[i].ParameterName.Substring(1).ToLower();
                    if (dict.ContainsKey(pName))
                        cmd.Parameters[i].Value = (dict[pName] == null ? DBNull.Value : dict[pName]);
                }
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                }
            }
        }

        public static void WriteWebDavLog(string databaseName, IDictionary<string, object> content)
        {
            using (var conn = new SqlConnection(Solution.CreateFromContext(databaseName).ConnectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("[admin].[WriteWebDavLog]", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlCommandBuilder.DeriveParameters(cmd);

                for (int i = 1; i < cmd.Parameters.Count; i++)
                {
                    String pName = cmd.Parameters[i].ParameterName.Substring(1).ToLower();
                    object value;
                    if (content.TryGetValue(pName, out value))
                        cmd.Parameters[i].Value = value ?? DBNull.Value;
                    else
                        cmd.Parameters[i].Value = DBNull.Value;
                }
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                }
            }
        }

        public static IDictionary<string, IList<string>> GetUserPushTokensByOs(String databaseName, Guid userId)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(Solution.CreateFromContext(databaseName).ConnectionString))
            {
                conn.Open();
                var cmd = new System.Data.SqlClient.SqlCommand("[admin].[GetUserPushTokens]", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                try
                {
                    System.Data.SqlClient.SqlCommandBuilder.DeriveParameters(cmd);
                }
                catch
                {
                    throw new Exception("Database does not support push notifications");
                }
                cmd.Parameters["@UserId"].Value = userId;

                var result = new Dictionary<string, IList<string>>();

                using (System.Data.SqlClient.SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read())
                    {
                        string token = r.GetString(0);
                        string os = r.GetString(1);
                        IList<string> tokens;
                        if (!result.TryGetValue(os, out tokens))
                            result.Add(os, tokens = new List<string>());
                        tokens.Add(token);
                    }

                return result;
            }
        }
    }
}
