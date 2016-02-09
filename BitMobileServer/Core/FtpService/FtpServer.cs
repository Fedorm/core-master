using System;
using System.Globalization;
using System.IO;
using RemObjects.InternetPack.Ftp.VirtualFtp;
using RemObjects.InternetPack.Ftp;

namespace FtpService
{
    public class FtpServer
    {
        private int passivePortFrom = 40000;
        public int PassivePortFrom
        {
            get { return passivePortFrom; }
            set { passivePortFrom = value; }
        }

        private int passivePortTo = 40019;
        public int PassivePortTo
        {
            get { return passivePortTo; }
            set { passivePortTo = value; }
        }

        private String passiveAddress;
        public String PassiveAddress
        {
            get { return passiveAddress; }
            set { passiveAddress = value; }
        }

        private readonly String rootPath;
        private readonly int port;
        private VirtualFolder rootFolder;

        public FtpServer(String rootPath, int port)
        {
            this.rootPath = rootPath;
            this.port = port;
        }

        public void Start()
        {
            rootFolder = new VirtualFolder(null, "root");
            SetReadOnly(rootFolder, "root");

            VirtualFtpServer fFtpServer = new VirtualFtpServer();
            fFtpServer.Detailed500Errors = true;
            fFtpServer.Port = port;
            fFtpServer.PassiveAddress = passiveAddress;
            fFtpServer.PassivePortFrom = passivePortFrom;
            fFtpServer.PassivePortTo = passivePortTo;
            fFtpServer.Timeout = 60 * 1000; /* 1 minute */
            if (fFtpServer.BindingV4 != null)
                fFtpServer.BindingV4.ListenerThreadCount = 10;
            fFtpServer.RootFolder = rootFolder;
            fFtpServer.UserManager = new UserManagerEx(fFtpServer, rootPath);
            fFtpServer.ServerName = "BitMobile FTP Server";
            fFtpServer.OnUserLogin += OnUserLogin;
            fFtpServer.OnChangeDirectory += OnChangeDirectory;

            fFtpServer.Open();
        }

        private void SetReadOnly(FtpFolder folder, String userName)
        {
            folder.UserRead = true;
            folder.UserWrite = false;
            folder.WorldRead = false;
            folder.WorldWrite = false;
            folder.GroupRead = false;
            folder.GroupWrite = false;
            folder.OwningUser = userName;
        }

        private void SetReadOnly(FtpFile folder, String userName)
        {
            folder.UserRead = true;
            folder.UserWrite = false;
            folder.WorldRead = false;
            folder.WorldWrite = false;
            folder.GroupRead = false;
            folder.GroupWrite = false;
            folder.OwningUser = userName;
        }

        private void OnUserLogin(object sender, FtpUserLoginEventArgs e)
        {
            if (e.LoginOk)
            {
                VirtualFtpServer server = (VirtualFtpServer)sender;
                VirtualFtpSession session = (VirtualFtpSession)e.Session;
                session.IsFileAdmin = e.UserName.Equals(server.RootFolder.Name);
                session.CurrentFolder = GetUserFolder(session);
            }
        }

        private void OnChangeDirectory(object sender, FtpChangeDirectoryArgs e)
        {
            VirtualFtpServer server = (VirtualFtpServer)sender;
            VirtualFtpSession session = (VirtualFtpSession)e.Session;
            if (e.NewDirectory.Equals("/"))
            {
                session.CurrentFolder = server.RootFolder.GetSubFolder(session.Username, session);
                session.Directory = String.Format(@"/{0}/", session.Username);
            }
        }

        private IFtpFolder GetUserFolder(VirtualFtpSession session)
        {
            IFtpFolder userFolder = rootFolder.GetSubFolder(session.Username, session);
            if (userFolder != null)
                return userFolder;
            else
            {
                if (session.IsFileAdmin)
                    return CreateRootFolder(session);

                if (session.Username.ToLower().EndsWith("_admin"))
                    return CreateAdminFolder(session);

                return CreateUserFolder(session);
            }
        }

        private IFtpFolder CreateRootFolder(VirtualFtpSession session)
        {
            VirtualFolder f = new VirtualFolder(rootFolder, session.Username);
            SetReadOnly(f, session.Username);

            foreach (String dir in System.IO.Directory.EnumerateDirectories(rootPath))
            {
                String name = new System.IO.DirectoryInfo(dir).Name;
                DiscFolder df = new DiscFolder(null, name, dir);
                SetReadOnly(df, session.Username);
                f.Add(df);
            }
            rootFolder.Add(f);
            return f;
        }

        private IFtpFolder CreateAdminFolder(VirtualFtpSession session)
        {
            String solutionName = session.Username.Split('_')[0];

            CreateFileSystemFoldersIfNotExists(solutionName);

            VirtualFolder f = new VirtualFolder(rootFolder, session.Username);
            SetReadOnly(f, session.Username);

            DiscFolder df;
            df = new DiscFolder(null, "exchange", GetExchangeFolder(solutionName));
            SetReadOnly(df, session.Username);
            df.UserWrite = true;
            f.Add(df);

            CatalogFile cfs = new CatalogFile(null, "exchange.txt", GetExchangeFolder(solutionName));
            SetReadOnly(cfs, session.Username);
            f.Add(cfs);

            df = new DiscFolder(null, "shared", GetSharedFolder(solutionName));
            SetReadOnly(df, session.Username);
            df.UserWrite = true;
            f.Add(df);

            cfs = new CatalogFile(null, "shared.txt", GetSharedFolder(solutionName));
            SetReadOnly(cfs, session.Username);
            f.Add(cfs);

            df = new DiscFolder(null, "private", GetPrivateFolder(solutionName, ""));
            SetReadOnly(df, session.Username);
            df.UserWrite = true;
            f.Add(df);

            cfs = new CatalogFile(null, "private.txt", GetPrivateFolder(solutionName, ""));
            SetReadOnly(cfs, session.Username);
            f.Add(cfs);

            rootFolder.Add(f);
            return f;
        }

        private IFtpFolder CreateUserFolder(VirtualFtpSession session)
        {
            String[] arr = session.Username.Split('_');
            String solutionName = arr[0];
            String userFolderName = arr[1];

            CreateFileSystemFoldersIfNotExists(solutionName);

            VirtualFolder f = new VirtualFolder(rootFolder, session.Username);
            SetReadOnly(f, session.Username);

            String sharedFolder = GetSharedFolder(solutionName);
            DiscFolder df;
            df = new DiscFolder(null, "shared", sharedFolder);
            SetReadOnly(df, session.Username);
            f.Add(df);

            CatalogFile cfs = new CatalogFile(null, "shared.txt", sharedFolder);
            SetReadOnly(cfs, session.Username); 
            f.Add(cfs);

            String privateFolder = GetPrivateFolder(solutionName, userFolderName);
            df = new DiscFolder(null, "private", privateFolder);
            SetReadOnly(df, session.Username);
            df.UserWrite = true;
            f.Add(df);

            CatalogFile cfp = new CatalogFile(null, "private.txt", privateFolder);
            SetReadOnly(cfp, session.Username);
            f.Add(cfp);

            rootFolder.Add(f);
            return f;
        }

        private void CreateFileSystemFoldersIfNotExists(String solutionName)
        {
            String solutionFolder = String.Format(@"{0}\{1}", rootPath, solutionName);
            String fileSystemFolder = String.Format(@"{0}\filesystem", solutionFolder);
            if (!System.IO.Directory.Exists(fileSystemFolder))
                System.IO.Directory.CreateDirectory(fileSystemFolder);
            String sharedFolder = String.Format(@"{0}\shared", fileSystemFolder);
            if (!System.IO.Directory.Exists(sharedFolder))
                System.IO.Directory.CreateDirectory(sharedFolder);
            String privateFolder = String.Format(@"{0}\private", fileSystemFolder);
            if (!System.IO.Directory.Exists(privateFolder))
                System.IO.Directory.CreateDirectory(privateFolder);
            String exchangeFolder = String.Format(@"{0}\exchange", fileSystemFolder);
            if (!System.IO.Directory.Exists(exchangeFolder))
                System.IO.Directory.CreateDirectory(exchangeFolder);
        }

        private String GetSharedFolder(String solutionName)
        {
            return String.Format(@"{0}\{1}\filesystem\shared", rootPath, solutionName);
        }

        private String GetExchangeFolder(String solutionName)
        {
            return String.Format(@"{0}\{1}\filesystem\exchange", rootPath, solutionName);
        }

        private String GetPrivateFolder(String solutionName, String userName)
        {
            userName = String.IsNullOrEmpty(userName) ? userName : @"\" + userName;
            String result = String.Format(@"{0}\{1}\filesystem\private{2}", rootPath, solutionName, userName);
            if (!System.IO.Directory.Exists(result))
                System.IO.Directory.CreateDirectory(result);
            return result;
        }

    }

    public class UserManagerEx : UserManager
    {
        private VirtualFtpServer server;
        private String rootPath;

        public UserManagerEx(VirtualFtpServer server, String rootPath):
            base()
        {
            this.server = server;
            this.rootPath = rootPath;
        }

        public override bool CheckLogin(string username, string password, VirtualFtpSession session)
        {
            if (username.ToLower().Equals(server.RootFolder.Name.ToLower()))
            {
                return Common.Solution.CreateFromContext("").RootPassword.Equals(password);
            }

            String[] arr = username.Split('_');
            if (arr.Length == 2)
            {
                String solutionName = arr[0];

                if (arr[1].ToLower().Equals("admin"))
                {

                    foreach (String dir in System.IO.Directory.EnumerateDirectories(rootPath))
                    {
                        String name = new System.IO.DirectoryInfo(dir).Name;
                        if (solutionName.ToLower().Equals(name.ToLower()))
                        {
                            return Common.Solution.CreateFromContext(solutionName).SolutionPassword.Equals(password);
                        }
                    }
                    return false;
                }

                Guid userId;
                if (System.Guid.TryParse(arr[1], out userId))
                {
                    return Common.Logon.UserExists(solutionName, userId, password);
                }
            }

            return false;
        }
    }

    public class CatalogFile : EmptyFile
    {
        private String root;

        public CatalogFile(IFtpFolder parent, String name, String root)
            :base(parent, name)
        {
            this.root = root;
        }

        public override void GetFile(Stream destination)
        {
            using (var wr = new StreamWriter(destination))
            {
                foreach (String file in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
                {
                    string date = new FileInfo(file).LastWriteTimeUtc.ToString(@"yyyy\.MM\.dd hh:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
                    string size = new FileInfo(file).Length.ToString(CultureInfo.InvariantCulture);
                    wr.WriteLine("{0}|{1}|{2}", file.Replace(root, ""), date, size);
                }
                wr.Flush();
            }
        }
    }
}
