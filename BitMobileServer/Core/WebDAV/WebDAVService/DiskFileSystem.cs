using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Specialized;

namespace BMWebDAV
{
    public class DiskFileSystem : FileSystem
    {
        private String _root;

        public DiskFileSystem(String root) { _root = root; }

        public override bool FileExists(String path)
        {
            return File.Exists(Path.Combine(_root,path));
        }

        public override bool DirectoryExists(String path)
        {
            return Directory.Exists(Path.Combine(_root,path));
        }

        public override void DeleteFile(String path)
        {
            if (FileExists(path)) {
                FileInfo _fileInfo=new FileInfo(Path.Combine(_root,path));
                if (_fileInfo!=null)
                    _fileInfo.Delete();
            }
        }

        public override void DeleteDirectory(String path)
        {
            if (DirectoryExists(path)) {
                DirectoryInfo _dirInfo=new DirectoryInfo(Path.Combine(_root,path));
                if (_dirInfo!=null)
                    _dirInfo.Delete(true);
            }
        }

        public override System.IO.Stream OpenRead(String path)
        {
            Stream result=null;
            if (FileExists(path)) {
                FileInfo _fileInfo=new FileInfo(Path.Combine(_root,path));
                if (_fileInfo!=null)
                    result=_fileInfo.OpenRead();
            }
            return result;
        }

        public override System.IO.Stream OpenWrite(String path)
        {
            Stream result=null;
                FileInfo _fileInfo=new FileInfo(Path.Combine(_root,path));
                if (_fileInfo!=null)
                    result=_fileInfo.OpenWrite();
            return result;
        }

        public override void CreateSubDirectory(String path, String subDir)
        {
            if (DirectoryExists(path)) {
                DirectoryInfo _dirInfo=new DirectoryInfo(Path.Combine(_root,path));
                if (_dirInfo!=null)
                    _dirInfo.CreateSubdirectory(subDir);
            }
        }

        public override List<ItemInfo> EnumerateFiles(String path, bool recurse = false)
        {
            List<ItemInfo> result=new List<ItemInfo>();

            foreach (String dir in System.IO.Directory.EnumerateFiles(Path.Combine(_root,path),"*.*",recurse ? SearchOption.AllDirectories: SearchOption.TopDirectoryOnly))
            {
                FileInfo _fileInfo= new  System.IO.FileInfo(dir);
                result.Add(new ItemInfo { Name = recurse ? _fileInfo.FullName.Replace(Path.Combine(_root, path), "") : _fileInfo.Name, CreationTime = _fileInfo.CreationTimeUtc, LastWriteTime = _fileInfo.LastWriteTimeUtc, Length = _fileInfo.Length, Parent = _fileInfo.Directory.Name, SubParent = _fileInfo.Directory.Parent.Name });
                
            }
            return result;
        }

        public override List<ItemInfo> EnumerateDirectories(String path)
        {
            List<ItemInfo> result=new List<ItemInfo>();
            foreach (String dir in System.IO.Directory.EnumerateDirectories(Path.Combine(_root,path)))
            {
                DirectoryInfo _dirInfo= new  System.IO.DirectoryInfo(dir);
                result.Add(new ItemInfo { Name = _dirInfo.Name, CreationTime = _dirInfo.CreationTimeUtc, LastWriteTime = _dirInfo.LastWriteTimeUtc, Length = _dirInfo.GetDirectories().Length + _dirInfo.GetFiles().Length });
                
            }
            return result;
        }

        public override FileSystem.ItemInfo GetFileInfo(string path)
        {
            FileInfo _fileInfo=new FileInfo(Path.Combine(_root,path));
            return new ItemInfo { Name =  _fileInfo.Name, CreationTime = _fileInfo.CreationTimeUtc, LastWriteTime = _fileInfo.LastWriteTimeUtc, Length = _fileInfo.Length };
        }

        public override FileSystem.ItemInfo GetFileInfo2(string path)
        {
            FileInfo _fileInfo = new FileInfo(Path.Combine(_root, path));
            return new ItemInfo { Name = Path.Combine(_root, path), CreationTime = _fileInfo.CreationTimeUtc, LastWriteTime = _fileInfo.LastWriteTimeUtc, Length = _fileInfo.Length };
        }

        public override FileSystem.ItemInfo GetDirectoryInfo(string path)
        {
            DirectoryInfo _dirInfo = new DirectoryInfo(Path.Combine(_root, path));
            return new ItemInfo { Name =  _dirInfo.Name, CreationTime = _dirInfo.CreationTimeUtc, LastWriteTime = _dirInfo.LastWriteTimeUtc, Length = _dirInfo.GetDirectories().Length + _dirInfo.GetFiles().Length };
        }
    }

}
