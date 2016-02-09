using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BMWebDAV
{
    public class VirtualDirectory
    {
        private String solution;
        public List<FileItem> items;

        public VirtualDirectory(FileSystem fileSystem, String solution)
        {
            this.items = new List<FileItem>();
            this.solution = solution;
            this._fileSystem = fileSystem;
        }

        public void AddFolder(String name, String relativePath, String accessRights)
        {
            items.Add(new FileItem { Directory = this, Name = name, RelativePath = relativePath, AccessRights = accessRights });
        }

        public void AddVirtualFile(String name, String relativePath, VirtualFile file)
        {
            file.Name = name;
            file.Directory = this;
            file.RelativePath = relativePath;
            file.AccessRights = "r";
            items.Add(file);
        }

        public FileItem GetItem(String path)
        {
            FileItem item = null;
            int counter = 0;
            String rest="";

            String[] pathArr=path.Split('/');

            for (int pathIdx = 0; pathIdx < pathArr.Length; pathIdx++)
            {
                if (string.IsNullOrEmpty(pathArr[pathIdx])) 
                    continue;
                if (counter == 0)
                {
                    for (int itemsIdx = 0; itemsIdx < items.Count; itemsIdx++)
                    {
                        if (items[itemsIdx].Name.ToLower().Equals(pathArr[pathIdx].ToLower()))
                        {
                            item = items[itemsIdx];
                            break;
                        }
                    }
                }
                else
                    rest = rest + @"\" + pathArr[pathIdx];
                counter++;
            }

            if ((counter > 1) && (item != null)) 
                item.RelativePath = item.RelativePath + @"\" + rest;
            else if ((counter==1) && (item != null)) 
                item.AccessRights="r";
            return item;
        }

        
        public String UserName { get; set; }
        public String PhysicalPath { get; set; }
        public FileSystem _fileSystem;
        public String SolutionPath
        {
            get
            {
                return System.IO.Path.Combine(PhysicalPath, solution); 
            }
        }

        public static String PathInfo(String solutionName)
        {
            String result = HttpContext.Current.Request.Path.ToLower();
            String tobeExclueded = String.Format("/{0}/webdav", solutionName);
            
            int idx = result.IndexOf(tobeExclueded);
            if (idx == -1)
                throw new Exception("Invalid path !");
            return result.Substring(idx + tobeExclueded.Length);
        }

    }

    public class FileItem
    {
        public VirtualDirectory Directory { get; set; }
        public String Name { get; set; }
        public String RelativePath { get; set; }
        public String AccessRights { get; set; }
    }

    public abstract class VirtualFile : FileItem
    {
        public abstract void GetData(System.IO.Stream stream);
    }
}
