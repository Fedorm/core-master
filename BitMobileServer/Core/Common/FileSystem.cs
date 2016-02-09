using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Specialized;

namespace BMWebDAV
{
    public abstract class FileSystem
    {
        public class ItemInfo
        {
            public String Name;
            public DateTime CreationTime;
            public DateTime LastWriteTime;
            public long Length=0;
            public String Parent;
            public String SubParent;
        }

        public abstract bool FileExists(String path);
        public abstract bool DirectoryExists(String path);
        public abstract void DeleteFile(String path);
        public abstract void DeleteDirectory(String path);
        public abstract Stream OpenRead(String path);
        public abstract Stream OpenWrite(String path);
        public abstract void CreateSubDirectory(String path, String subDir);
        public abstract List<ItemInfo> EnumerateDirectories(String path);
        public abstract List<ItemInfo> EnumerateFiles(String path, bool recurse = false);
        public abstract ItemInfo GetFileInfo(String path);
        public abstract ItemInfo GetFileInfo2(String path);
        public abstract ItemInfo GetDirectoryInfo(String path);
    }
}
