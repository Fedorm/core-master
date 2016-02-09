using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Collections.Generic;

using System.ServiceModel;
using System.ServiceModel.Activation;
using FileHelperForCloud;
using System.Net;
using System.ServiceModel.Web;
using System.Web;
using System.Text;


namespace SystemService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SystemSyncService : ISystemRequestHandler
    {
        public SystemSyncService(String scope, System.Net.NetworkCredential credential)
            : base()
        {
            Common.Logon.CheckRootCredential(scope, credential);
        }

        public Stream Version()
        {
            return Common.Utils.MakeTextAnswer("BitMobile server (ver 2.506)");
        }

        public Stream Licenses()
        {
            String dir = Common.Crypt.GetLicensesPath();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (System.IO.Directory.Exists(dir))
            {
                foreach (Common.LicenseInfo li in Common.Crypt.GetLicenses())
                {
                    sb.AppendLine(String.Format("SERVER:{0}", li.Server));
                    sb.AppendLine(String.Format("ID:{0}", li.Id));
                    sb.AppendLine(String.Format("NAME:{0}", li.Name));
                    sb.AppendLine(String.Format("QTY:{0}", li.Qty));
                    sb.AppendLine(String.Format("EXPIREDATE:{0}", li.ExpireDate.ToShortDateString()));
                    sb.AppendLine();
                }
            }
            if (sb.Length == 0)
                sb.AppendLine("no licenses found");
            return Common.Utils.MakeTextAnswer(sb.ToString());
        }

        public Stream AddLicense(Stream messageBody)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(messageBody);

                String host = String.Format("http://{0}", doc.DocumentElement.ChildNodes[0].InnerText);
                String id = doc.DocumentElement.ChildNodes[1].InnerText;
                String uri = String.Format("{0}/license/activatelicense?key={1}", host, HttpUtility.UrlEncode(Convert.ToBase64String(Common.Crypt.GetKey())));

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
                req.Timeout = 15000; //15 sec
                req.Method = "POST";

                using (Stream requestStream = req.GetRequestStream())
                {
                    MemoryStream ms = new MemoryStream();
                    doc.Save(ms);
                    ms.Position = 0;
                    ms.CopyTo(requestStream);
                }

                WebResponse resp = req.GetResponse();

                HttpStatusCode lastStatusCode = ((HttpWebResponse)resp).StatusCode;
                Stream rs = resp.GetResponseStream();
                if (lastStatusCode == HttpStatusCode.OK)
                {
                    if (!System.IO.Directory.Exists(Common.Crypt.GetLicensesPath()))
                        System.IO.Directory.CreateDirectory(Common.Crypt.GetLicensesPath());
                    using (FileStream fs = System.IO.File.OpenWrite(String.Format("{0}\\{1}", Common.Crypt.GetLicensesPath(), id)))
                    {
                        rs.CopyTo(fs);
                    }

                    Common.Crypt.ReadLicenses(); //refresh

                    return Common.Utils.MakeTextAnswer("ok");
                }
                else
                {
                    return Common.Utils.MakeTextAnswer(StringFromStream(rs));
                }
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e);
            }
        }

        public Stream CreateSolution(String scope)
        {
            try
            {
                String pattern = @"^[A-Za-z]+[A-Za-z\d]*$";
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern);
                if (!regex.IsMatch(scope))
                    throw new Exception(String.Format("Solution name should start with symbol and contain only symbols or digits", scope));
                if (scope.ToLower().Equals("root"))
                    throw new Exception(String.Format("Prohibited name", scope));

                String path = Common.Solution.GetSolutionFolder(scope);

                if (System.IO.Directory.Exists(path))
                    throw new Exception(String.Format("Solution '{0}' already exists", scope));

                new CodeFactory.Builder().CreateSolutionFolders(path);

                EndPointHelper.CreateEndPoints(scope);

                return Common.Utils.MakeTextAnswer("ok");
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, scope);
            }
        }

        public Stream MoveSolution(String fromName, String toName)
        {
            try
            {
                String fromPath = Common.Solution.GetSolutionFolder(fromName);
                if (!System.IO.Directory.Exists(fromPath))
                    throw new Exception(String.Format("Solution '{0}' does not exist", fromName));

                String toPath = Common.Solution.GetSolutionFolder(toName);
                if (!System.IO.Directory.Exists(toPath))
                    throw new Exception(String.Format("Solution '{0}' does not exist", toName));

                Common.Solution.ChangeHostState(toName, "stop");

                DirectoryDelete(toPath);
                DirectoryCopy(fromPath, toPath, true);

                new CodeFactory.Builder().Build(Common.Solution.CreateFromContext(toName), false);

                Common.Solution.ChangeHostState(toName, "start");

                return Common.Utils.MakeTextAnswer("ok");
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e, fromName);
            }
        }

        public Stream Solutions()
        {
            String solutionFolder = Common.Solution.GetSolutionFolder("");
            String result = "";

            //solutions
            foreach (String dir in System.IO.Directory.EnumerateDirectories(solutionFolder))
            {
                if (result != "")
                    result += ";";
                result += new System.IO.DirectoryInfo(dir).Name;
            }

            return Common.Utils.MakeTextAnswer(result);
        }

        public Stream RemoveSolutions()
        {
            String solutionFolder = Common.Solution.GetSolutionFolder("");

            //solutions
            foreach (String dir in System.IO.Directory.EnumerateDirectories(solutionFolder))
            {
                System.IO.Directory.Delete(dir, true);
            }

            return Common.Utils.MakeTextAnswer("ok");
        }

        public Stream RemoveSolution(string name)
        {
            String solutionFolder = Common.Solution.GetSolutionFolder(name);
            System.IO.Directory.Delete(solutionFolder, true);

            return Common.Utils.MakeTextAnswer("ok");
        }

        public Stream SetPassword(string name, String password)
        {
            try
            {
                if (!System.IO.Directory.Exists(Common.Solution.GetSolutionFolder(name)))
                {
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
                    return Common.Utils.MakeTextAnswer("solution {0} not found", name);
                }

                CheckPasswordPolicy(password);
                String accessFolder = String.Format(@"{0}\access", Common.Solution.GetSolutionFolder(name));

                if (!System.IO.Directory.Exists(accessFolder))
                    System.IO.Directory.CreateDirectory(accessFolder);
                System.IO.File.WriteAllText(String.Format(@"{0}\password.txt", accessFolder), password);
                return Common.Utils.MakeTextAnswer("ok");
            }
            catch (Exception e)
            {
                return Common.Utils.MakeTextAnswer(e.Message);
            }
        }

        public Stream GetPassword(string name)
        {
            String solutionFolder = Common.Solution.GetSolutionFolder(name);
            if (!System.IO.Directory.Exists(solutionFolder))
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
                return Common.Utils.MakeTextAnswer("solution {0} not found", name);
            }

            String pwd = Common.Solution.CreateFromContext(name).SolutionPassword;
            if (String.IsNullOrEmpty(pwd))
                return Common.Utils.MakeTextAnswer("no password defined");
            else
                return Common.Utils.MakeTextAnswer(pwd);
        }

        public Stream SetLicenses(string name, String licenses)
        {
            if (!System.IO.Directory.Exists(Common.Solution.GetSolutionFolder(name)))
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
                return Common.Utils.MakeTextAnswer("solution {0} not found", name);
            }

            String accessFolder = String.Format(@"{0}\access", Common.Solution.GetSolutionFolder(name));

            if (!System.IO.Directory.Exists(accessFolder))
                System.IO.Directory.CreateDirectory(accessFolder);
            System.IO.File.WriteAllText(String.Format(@"{0}\licenses.txt", accessFolder), licenses);

            if (System.IO.File.Exists(String.Format(@"{0}\devices.txt", accessFolder)))
                System.IO.File.Delete(String.Format(@"{0}\devices.txt", accessFolder));

            return Common.Utils.MakeTextAnswer("ok");
        }

        public static void DirectoryDelete(string dirName)
        {
            try
            {
                string[] files = Directory.GetFiles(dirName);
                string[] dirs = Directory.GetDirectories(dirName);

                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }

                foreach (string dir in dirs)
                {
                    DirectoryDelete(dir);
                }

                Directory.Delete(dirName, false);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void CheckPasswordPolicy(String password)
        {
            if (!String.IsNullOrEmpty(password))
            {
                if (password.Length >= 8)
                {
                    int flag = 0;
                    foreach (Char c in password)
                    {
                        if (Char.IsDigit(c))
                        {
                            flag = flag | 1;
                            continue;
                        }

                        if (Char.IsLower(c))
                        {
                            flag = flag | 2;
                            continue;
                        }

                        if (Char.IsUpper(c))
                        {
                            flag = flag | 4;
                            continue;
                        }
                    }
                    if (flag == (1 + 2 + 4))
                        return;
                }
            }
            throw new Exception("Password does not meet security policy");
        }

        private static String StringFromStream(Stream s)
        {
            using (StreamReader sr = new StreamReader(s, Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }
    }

}
