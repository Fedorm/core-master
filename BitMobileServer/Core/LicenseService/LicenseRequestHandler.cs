using CodeFactory.DatabaseFactory;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LicenseService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class LicenseRequestHandler : ILicenseRequestHandler
    {
        private String scope;
        private System.Net.NetworkCredential credential;

        public LicenseRequestHandler(String scope, System.Net.NetworkCredential credential)
        {
            this.scope = scope;
            this.credential = credential;
        }

        public System.IO.Stream InitService()
        {
            Trace.TraceInformation("Processing InitService request");

            try
            {
                Common.Logon.CheckRootCredential(scope, credential);
            }
            catch (UnauthorizedAccessException e)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = e.Message;
                return null;
            }
            Common.Solution solution = CreateSolution();

            if (CheckIfDbExists(solution))
            {
                return MakeErrorAnswer("License service is already initialized");
            }
            else
            {
                try
                {
                    DatabaseFactory dbf = new DatabaseFactory(solution.DatabaseServer);

                    Assembly a = Assembly.GetExecutingAssembly();
                    Trace.TraceInformation("loaded assembly {0} from {1}", a.FullName, a.CodeBase);

                    //create database
                    String script = "";
                    if (solution.IsAsured)
                        script = new StreamReader(a.GetManifestResourceStream("LicenseService.Database.createDatabaseAzure.sql")).ReadToEnd();
                    else
                        script = new StreamReader(a.GetManifestResourceStream("LicenseService.Database.createDatabase.sql")).ReadToEnd();
                    script = script.Replace("%databaseName%", solution.DatabaseName);
                    Trace.TraceInformation("preparing to run script {0}", script);
                    dbf.RunScript(script);
                    Trace.TraceInformation("script1 succeeded");

                    SqlConnection.ClearAllPools(); // dirty fix for next connection not knowing about created database

                    //run script
                    dbf = new DatabaseFactory(solution.ConnectionString);
                    Trace.TraceInformation("got new dbf");
                    script = new StreamReader(a.GetManifestResourceStream("LicenseService.Database.database.sql")).ReadToEnd();
                    Trace.TraceInformation("preparing to run script {0}", script);
                    dbf.RunScript(script);
                    Trace.TraceInformation("script2 succeeded");

                    return MakeTextAnswer("ok");
                }
                catch (Exception e)
                {
                    return MakeExceptionAnswer(e);
                }
            }
        }

        public Stream CreateLicense()
        {
            Trace.TraceInformation("Processing CreateLicense request");

            try
            {
                Common.Logon.CheckRootCredential(scope, credential);
            }
            catch (UnauthorizedAccessException e)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = e.Message;
                return null;
            }

            Common.Solution solution = CreateSolution();

            try
            {
                if (!CheckIfDbExists(solution))
                    return MakeErrorAnswer("License service is not initialized");

                Dictionary<String, String> ps = GetParameters();

                String server = GetServer();
                Guid id = Guid.NewGuid();
                String ln = ps["ln"];
                int lqty = int.Parse(ps["lqty"]);
                DateTime le = DateTime.Parse(ps["le"]);

                CreateLicenseDbRecord(solution, server, id, ln, lqty, le);

                return CreateLicenseFile(server, id, ln, lqty, le);
            }
            catch (Exception e)
            {
                Trace.TraceError("EXCEPTION {0}", Common.Utils.MakeDetailedExceptionString(e));
                throw;
            }
        }

        public System.IO.Stream ActivateLicense(System.IO.Stream messageBody)
        {
            Trace.TraceInformation("Processing ActivateLicense request");

            Common.Solution solution = CreateSolution();

            byte[] key;
            try
            {
                Trace.TraceInformation("Activation request with params: {0}",string.Join("", GetParameters().Select(p => string.Format("\n{0}={1}", p.Key, p.Value))));
                key = Convert.FromBase64String(GetParameters()["key"]);
                Trace.TraceInformation("Trying to activate license for key {0}", key);
            }
            catch
            {
                return MakeErrorAnswer("invalid key");
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(messageBody);

            XmlNode node = doc.DocumentElement;
            String server = node.ChildNodes[0].InnerText;
            Guid id = Guid.Parse(node.ChildNodes[1].InnerText);
            String name = node.ChildNodes[2].InnerText;
            int qty = int.Parse(node.ChildNodes[3].InnerText);
            DateTime expireDate = DateTime.Parse(node.ChildNodes[4].InnerText);

            if (server != GetServer())
            {
                Trace.TraceError("Requested license for server {0}, current server is {1}", server, GetServer());
                return MakeErrorAnswer("Bad license file");
            }

            bool exists;
            bool activated;
            GetLicenseInfo(solution, id, out exists, out activated);

            if (!exists)
                return MakeErrorAnswer("Bad license file");

            if (activated)
                return MakeErrorAnswer("License is already activated");

            ActivateLicenseDbRecord(solution, server, id, name, qty, expireDate);

            GetLicenseInfo(solution, id, out exists, out activated);

            if (activated)
            {
                Trace.TraceInformation("License {0} successfully activated", id);
                return Crypt.EncryptStream(key, GetDocumentAsStream(doc));
            }
            else
                return MakeErrorAnswer("License activation failure");
        }

        private bool CheckIfDbExists(Common.Solution solution)
        {
            using (SqlConnection conn = new SqlConnection(solution.ConnectionString))
            {
                try
                {
                    conn.Open();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        private void CreateLicenseDbRecord(Common.Solution solution, String server, Guid id, String name, int qty, DateTime expireDate)
        {
            Trace.TraceInformation("Trying to add license db record for license {0}, name:{1}, quantity:{2}, expireDate{3}", id, name, qty, expireDate);

            using (SqlConnection conn = new SqlConnection(solution.ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("dbo.CreateLicense", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters["@Id"].Value = id;
                cmd.Parameters["@Server"].Value = server;
                cmd.Parameters["@Name"].Value = name;
                cmd.Parameters["@Qty"].Value = qty;
                cmd.Parameters["@ExpireDate"].Value = expireDate;
                cmd.ExecuteNonQuery();
            }
        }

        private void ActivateLicenseDbRecord(Common.Solution solution, String server, Guid id, String name, int qty, DateTime expireDate)
        {
            Trace.TraceInformation("Trying to add activation db record for license {0}, name:{1}, quantity:{2}, expireDate{3}", id, name, qty, expireDate);

            using (SqlConnection conn = new SqlConnection(solution.ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("dbo.ActivateLicense", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters["@Id"].Value = id;
                cmd.Parameters["@Server"].Value = server;
                cmd.Parameters["@Name"].Value = name;
                cmd.Parameters["@Qty"].Value = qty;
                cmd.Parameters["@ExpireDate"].Value = expireDate;
                cmd.ExecuteNonQuery();
            }
        }

        private void GetLicenseInfo(Common.Solution solution, Guid id, out bool exists, out bool activated)
        {
            exists = false;
            activated = false;
            Trace.TraceInformation("Trying to get info for license {0}", id);

            using (SqlConnection conn = new SqlConnection(solution.ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("dbo.GetLicenseInfo", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters["@Id"].Value = id;

                SqlDataReader r = cmd.ExecuteReader();
                if (!r.Read())
                {
                    Trace.TraceError("Could not find info for license {0}", id);
                    return;
                }
                else
                {
                    exists = true;
                    activated = r.GetBoolean(r.GetOrdinal("Activated"));
                    Trace.TraceInformation("License {0} exists:{1}, activated:{2}", id, exists, activated);
                }
            }
        }

        private System.IO.Stream CreateLicenseFile(String server, Guid id, String name, int qty, DateTime expireDate)
        {
            Dictionary<String, String> attrs = new Dictionary<String, String>();
            attrs.Add("Server", server);
            attrs.Add("Id", id.ToString());
            attrs.Add("Name", name);
            attrs.Add("Qty", qty.ToString());
            attrs.Add("ExpireDate", expireDate.ToShortDateString());

            XmlDocument doc = new XmlDocument();

            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement license = doc.CreateElement(string.Empty, "License", string.Empty);
            doc.AppendChild(license);

            foreach (var attr in attrs)
            {
                XmlElement e = doc.CreateElement(string.Empty, attr.Key, string.Empty);
                e.InnerText = attr.Value;
                license.AppendChild(e);
            }

            return GetDocumentAsStream(doc);
        }

        private Stream GetDocumentAsStream(XmlDocument doc)
        {
            MemoryStream ms = new MemoryStream();
            using (StreamWriter w = new StreamWriter(ms, System.Text.Encoding.UTF8, 4096, true))
            {
                doc.Save(w);
            }
            ms.Position = 0;

            return ms;
        }

        private Common.Solution CreateSolution()
        {
            return Common.Solution.CreateFromContext("licensesDB");
        }

        private String GetServer()
        {
            Uri uri = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri;
            String result = uri.Host;
            for (int i = 1; i < uri.Segments.Length - 1; i++)
                result = result + "/" + uri.Segments[i];

            if (result.EndsWith("/"))
                result = result.Substring(0, result.Length - 1); //remove /

            return result;
        }

        private Dictionary<String, String> GetParameters()
        {
            Dictionary<String, String> result = new Dictionary<string, string>();
            var ps = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;
            for (int i = 0; i < ps.Count; i++)
            {
                String[] v = ps.GetValues(i);
                result.Add(ps.GetKey(i), v[0]);
            }

            return result;
        }

        public Stream MakeExceptionAnswer(Exception e)
        {
            Trace.TraceError("EXCEPTION {0}", Common.Utils.MakeDetailedExceptionString(e));

            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
            return Common.Utils.MakeExceptionAnswer(e);
        }

        public Stream MakeTextAnswer(String s)
        {
            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            return Common.Utils.MakeTextAnswer(s);
        }

        public Stream MakeErrorAnswer(String s)
        {
            Trace.TraceError(s);
            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NonAuthoritativeInformation;
            return Common.Utils.MakeTextAnswer(s);
        }

    }
}
