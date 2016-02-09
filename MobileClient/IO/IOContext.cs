using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using BitMobile.Application;
using BitMobile.Application.Exceptions;
using BitMobile.Application.Translator;
using BitMobile.Common.Application;
using BitMobile.Common.IO;
using BitMobile.Common.Utils;

namespace BitMobile.IO
{
    // ReSharper disable once InconsistentNaming
    public class IOContext : IIOContext
    {
        private readonly IApplicationContext _context;

        public IOContext(IApplicationContext context)
        {
            _context = context;
        }

        public string PrivateDirectory { get { return Provider.PrivateDirectory; } }

        public string SharedDirectory { get { return Provider.SharedDirectory; } }

        public string LogDirectory { get { return Provider.LogDirectory; } }

        public ILocalProvider CreateLocalProvider(string root)
        {
            return new FileSystemProvider(this, _context.LocalStorage, root);
        }

        public IRemoteProvider CreateRemoteProvider(string root)
        {
            var uri = new UriBuilder(_context.Settings.BaseUrl).Uri;
            string solutionName = _context.Settings.SolutionName;
            if (_context.Settings.WebDavDisabled)
            {
                string userName = string.Format("{0}_{1}", solutionName, _context.Dal.UserId);
                string address = string.Format("ftp://{0}:{1}/", uri.Host, _context.Settings.FtpPort);
                var info = new FtpProvider.ConnectionInfo(address, userName, _context.Settings.Password);
                return new FtpProvider(info, root);
            }
            return CreateWebDavProvider(root);
        }

        public IRemoteProvider CreateWebDavProvider(string root)
        {
            string address = string.Format("{0}/webdav/", _context.Settings.BaseUrl.ToCurrentScheme(ApplicationContext.Current.Settings.HttpsDisabled));
            var info = new WebDavProvider.ConnectionInfo(address, _context.Dal.UserId.ToString(), _context.Settings.Password);
            return new WebDavProvider(info, root);
        }

        public bool Delete(string path, FileSystemItem type = FileSystemItem.Both)
        {
            path = PreparePath(path);

            if (type != FileSystemItem.Directory && File.Exists(path))
            {
                File.Delete(path);
                return true;
            }

            if (type != FileSystemItem.File && Directory.Exists(path))
            {
                RecursiveDeleteDirectory(path);
                return true;
            }

            return false;
        }

        public bool Exists(string path, FileSystemItem type = FileSystemItem.Both)
        {
            path = PreparePath(path);

            if (type != FileSystemItem.Directory && File.Exists(path))
                return true;
            if (type != FileSystemItem.File && Directory.Exists(path))
                return true;

            return false;
        }

        public bool CreateDirectory(string path)
        {
            path = PreparePath(path);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return true;
            }
            return false;
        }

        public void Copy(string source, string dest, bool overwrite = false, FileSystemItem type = FileSystemItem.Both)
        {
            source = PreparePath(source);
            dest = PreparePath(dest);

            if (type != FileSystemItem.Directory && File.Exists(source))
            {
                string destDir = Path.GetDirectoryName(dest);
                if (destDir == null)
                    throw new ArgumentException("Invalid destination path: " + dest);

                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                File.Copy(source, dest, overwrite);
            }
            else if (type != FileSystemItem.File && Directory.Exists(source))
            {
                DirectoryCopy(source, dest, overwrite);
            }
            else
                throw new FileNotFoundException("File not found", source);
        }

        public string TranslateLocalPath(string path)
        {
            return FileSystemProvider.TranslatePath(_context.LocalStorage, path);
        }

        public string GetSolutionName(Uri uri)
        {
            string solutionName = uri.Segments[uri.Segments.Length - 1];

            solutionName = solutionName.Replace("/", "");
            solutionName = solutionName.Replace("\\", "");

            return solutionName;
        }

        public FileStream FileStream(string path, FileMode fileMode)
        {
            path = PreparePath(path);

            return new FileStream(path, fileMode);
        }

        private static string PreparePath(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            string dir = Path.GetDirectoryName(path);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return path;
        }

        private void DirectoryCopy(string source, string dest, bool overwrite)
        {
            var dir = new DirectoryInfo(source);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!Directory.Exists(dest))
                Directory.CreateDirectory(dest);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(dest, file.Name);
                file.CopyTo(temppath, overwrite);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(dest, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, overwrite);
            }
        }

        static void RecursiveDeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (var dir in Directory.GetDirectories(path))
                    RecursiveDeleteDirectory(dir);

                foreach (var file in Directory.GetFiles(path))
                    File.Delete(file);

                Directory.Delete(path);
            }
        }
    }
}
