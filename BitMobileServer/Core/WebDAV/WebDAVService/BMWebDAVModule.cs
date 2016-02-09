using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using Sphorium.WebDAV.Server.Framework;
using Sphorium.WebDAV.Server.Framework.Classes;
using Sphorium.WebDAV.Server.Framework.Security;
using Common;
using System.Data.SqlClient;

namespace BMWebDAV
{
	public class BMWebDAVModule : WebDAVModule
	{
        public static FileSystem _fileSystem;
        public static List<string> _solutions = new List<string>();

        public static void Init(FileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public static void AddSolution(String solutionName)
        {
            _solutions.Add(solutionName.ToLower());
        }

        public static void RemoveSolution(String solutionName)
        {
            _solutions.Remove(solutionName);
        }

		public BMWebDAVModule()
			: base(Assembly.GetExecutingAssembly(), Authentication.Basic)
		{
			base.ProcessRequest += new ProcessRequestEventHandler(FileWebDAVModule_ProcessRequest);
			base.BasicAuthorization += new BasicAuthorizationEventHandler(FileWebDAVModule_BasicAuthorization);
			base.AuthenticateRequest += new AuthenticationEventHandler(FileWebDAVModule_Authentication);
		}

		private void FileWebDAVModule_Authentication(object sender, AuthenticationArgs e)
		{
            String solutionName = SolutionFromUri(e.RequestUri);
            if(!String.IsNullOrEmpty(solutionName))
            {
                e.ProcessAuthorization = true;
                e.Realm = solutionName;
            }
            else
                e.ProcessAuthorization = false;
		}


        private static void CreateFileSystemFoldersIfNotExists(String solutionName)
        {
            String fileSystemFolder = String.Format(@"{0}\filesystem", solutionName);
            if (!_fileSystem.DirectoryExists(fileSystemFolder))
                _fileSystem.CreateSubDirectory(solutionName, "filesystem");
            String sharedFolder = String.Format(@"{0}\shared", fileSystemFolder);
            if (!_fileSystem.DirectoryExists(sharedFolder))
                _fileSystem.CreateSubDirectory(fileSystemFolder, "shared");
            String privateFolder = String.Format(@"{0}\private", fileSystemFolder);
            if (!_fileSystem.DirectoryExists(privateFolder))
                _fileSystem.CreateSubDirectory(fileSystemFolder, "private");
            String logFolder = String.Format(@"{0}\log", fileSystemFolder);
            if (!_fileSystem.DirectoryExists(logFolder))
                _fileSystem.CreateSubDirectory(fileSystemFolder, "log");
            String exchangeFolder = String.Format(@"{0}\exchange", fileSystemFolder);
            if (!_fileSystem.DirectoryExists(exchangeFolder))
                _fileSystem.CreateSubDirectory(fileSystemFolder, "exchange");
        }

        private void FileWebDAVModule_BasicAuthorization(object sender, BasicAuthorizationArgs e)
        {
            e.Authorized = false;
            if (!String.IsNullOrEmpty(e.Realm))
            {
                if (!string.IsNullOrEmpty(e.UserName) && !string.IsNullOrEmpty(e.Password))
                {
                    if (e.UserName.ToLower().Equals("root"))
                    {
                        e.Authorized = Common.Solution.CreateFromContext(e.Realm).SolutionPassword.Equals(e.Password);
                        if (e.Authorized)
                        {
                            CreateFileSystemFoldersIfNotExists(e.Realm);
                            VirtualDirectory vdir = new VirtualDirectory(_fileSystem, e.Realm) { UserName = e.UserName };

                            String solutionFolder = String.Format(@"{0}", e.Realm);
                            vdir.AddFolder("access", System.IO.Path.Combine(solutionFolder, "access"), "r");
                            vdir.AddFolder("filesystem", System.IO.Path.Combine(solutionFolder, "filesystem"), "r");
                            vdir.AddFolder("log", System.IO.Path.Combine(solutionFolder, "log"), "r");
                            vdir.AddFolder("resource", System.IO.Path.Combine(solutionFolder, "resource"), "r");

                            e.UserData = vdir;
                        }
                        return;
                    }

                    if (e.UserName.ToLower().Equals("admin"))
                    {
                        e.Authorized = Common.Solution.CreateFromContext(e.Realm).SolutionPassword.Equals(e.Password);
                        if (e.Authorized)
                        {
                            CreateFileSystemFoldersIfNotExists(e.Realm);
                            VirtualDirectory vdir = new VirtualDirectory(_fileSystem, e.Realm) { UserName = e.UserName };

                            String solutionFolder = String.Format(@"{0}\filesystem", e.Realm);
                            vdir.AddFolder("shared", String.Format(@"{0}\shared", solutionFolder), "rw");
                            vdir.AddFolder("private", String.Format(@"{0}\private", solutionFolder), "rw");
                            vdir.AddFolder("exchange", String.Format(@"{0}\exchange", solutionFolder), "rw");

                            vdir.AddVirtualFile("exchange.txt", String.Format(@"{0}\exchange", solutionFolder), new CatalogFile());
                            vdir.AddVirtualFile("private.txt", String.Format(@"{0}\private", solutionFolder), new CatalogFile());
                            vdir.AddVirtualFile("shared.txt", String.Format(@"{0}\shared", solutionFolder), new CatalogFile());
                            vdir.AddVirtualFile("log.txt", String.Format(@"{0}\log", solutionFolder), new CatalogFile());

                            e.UserData = vdir;
                        }
                    return;
                    }

                    //user
                    Guid userId;
                    if (System.Guid.TryParse(e.UserName, out userId))
                    {
                        e.Authorized = Common.Logon.UserExists(e.Realm, userId, e.Password);
                        if (e.Authorized)
                        {
                            CreateFileSystemFoldersIfNotExists(e.Realm);
                            VirtualDirectory vdir = new VirtualDirectory(_fileSystem, e.Realm) { UserName = e.UserName };

                            String solutionFolder = String.Format(@"{0}\filesystem", e.Realm);

                            if (!_fileSystem.DirectoryExists(String.Format(@"{0}\private\{1}", solutionFolder, e.UserName)))
                                _fileSystem.CreateSubDirectory(String.Format(@"{0}\private", solutionFolder), e.UserName);
                            if (!_fileSystem.DirectoryExists(String.Format(@"{0}\log\{1}", solutionFolder, e.UserName)))
                                _fileSystem.CreateSubDirectory(String.Format(@"{0}\log", solutionFolder), e.UserName);
                            
                            vdir.AddFolder("shared", String.Format(@"{0}\shared", solutionFolder), "r");
                            vdir.AddFolder("private", String.Format(@"{0}\private\{1}", solutionFolder, e.UserName), "rw");
                            vdir.AddFolder("log", String.Format(@"{0}\log\{1}", solutionFolder, e.UserName), "rw");

                            if(Common.Logon.CheckIfFilterShared(e.Realm))
                                vdir.AddVirtualFile("shared.txt", String.Format(@"{0}\shared", solutionFolder), new CatalogFileFiltered(Common.Solution.CreateFromContext(e.Realm), userId));
                            else
                                vdir.AddVirtualFile("shared.txt", String.Format(@"{0}\shared", solutionFolder), new CatalogFile());

                            vdir.AddVirtualFile("private.txt", String.Format(@"{0}\private\{1}", solutionFolder, e.UserName), new CatalogFile());
                            vdir.AddVirtualFile("log.txt", String.Format(@"{0}\log\{1}", solutionFolder, e.UserName), new CatalogFile());

                            e.UserData = vdir;
                        }
                    }

                }
            }
        }

		private void FileWebDAVModule_ProcessRequest(object sender, DavModuleProcessRequestArgs e)
		{
            e.ProcessRequest = !String.IsNullOrEmpty(SolutionFromUri(e.RequestUri));
        }

        private static String SolutionFromUri(Uri uri)
        {
            if (_solutions != null)
            {
                String[] arr = uri.LocalPath.Split(new String[] { @"/" }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length >= 2)
                {
                    for (int i = 1; i < arr.Length; i++)
                    {
                        foreach (String s in _solutions)
                        {
                            if (arr[i].ToLower().Equals("webdav") && arr[i - 1].ToLower().Equals(s.ToLower()))
                            {
                                return s;
                            }
                        }
                    }
                }
            }
            return "";
        }

	}

    public class CatalogFile : VirtualFile
    {
        public override void GetData(Stream destination)
        {
            using (var wr = new StreamWriter(destination))
            {
                foreach (FileSystem.ItemInfo file in Directory._fileSystem.EnumerateFiles(RelativePath,true))
                {
                    string date = file.LastWriteTime.ToString(@"yyyy\.MM\.dd hh:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
                    string size = file.Length.ToString(CultureInfo.InvariantCulture);
                    wr.WriteLine("{0}|{1}|{2}", file.Name, date, size);
                }
                wr.Flush();
            }
        }
    }

    public class CatalogFileFiltered : VirtualFile
    {
        private Solution solution;
        private Guid userId;
        private Dictionary<String, SortedSet<Guid>> allowedIds;

        public CatalogFileFiltered(Solution solution, Guid userId)
        {
            this.solution = solution;
            this.userId = userId;
        }

        public override void GetData(Stream destination)
        {
            GetIds();

            using (var wr = new StreamWriter(destination))
            {
                foreach (FileSystem.ItemInfo file in Directory._fileSystem.EnumerateFiles(RelativePath, true))
                {
                    bool skip = false;
                    Guid id;
                    if (Guid.TryParse(file.Parent, out id))
                    {
                        String tableName = file.SubParent.ToLower();
                        if (allowedIds.ContainsKey(tableName))
                            skip = !allowedIds[tableName].Contains(id);
                    }

                    if (!skip)
                    {
                        string date = file.LastWriteTime.ToString(@"yyyy\.MM\.dd hh:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
                        string size = file.Length.ToString(CultureInfo.InvariantCulture);
                        wr.WriteLine("{0}|{1}|{2}", file.Name, date, size);
                    }
                }
                wr.Flush();
            }
        }

        private void GetIds()
        {
            allowedIds = new Dictionary<string, SortedSet<Guid>>();

            using (SqlConnection conn = new SqlConnection(solution.ConnectionString))
            {
                conn.Open();

                foreach (FileSystem.ItemInfo file in Directory._fileSystem.EnumerateDirectories(RelativePath))
                {
                    String tableName = file.Name.ToLower();

                    SqlCommand cmd = new SqlCommand(String.Format("{0}_adm_getfilterids", tableName), conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    try
                    {
                        SqlCommandBuilder.DeriveParameters(cmd);
                        cmd.Parameters["@UserId"].Value = userId;
                        SortedSet<Guid> list = new SortedSet<Guid>();
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                list.Add(r.GetGuid(0));
                            }
                        }
                        allowedIds.Add(tableName, list);
                    }
                    catch
                    {
                        //not supported
                    }
                }
            }

        }
    }

}
