using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Specialized;

namespace BMWebDAV
{
    public class AzureFileSystem :FileSystem
    {
        private CloudBlobContainer container;

        public AzureFileSystem(String connectionString, String containerName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference(containerName.ToLower());
            container.CreateIfNotExists();
        }

        private String NormalizePath(String path)
        {
            if (String.IsNullOrEmpty(path)) 
                return "/";
            String result=path.ToLower().Replace('\\', '/').Replace("//", "/");
            if (result.EndsWith("/"))
                return result.Substring(0,result.Length-1);
            else
                return result;
        }

        private String UnNormalizePath(String path)
        {
           if (String.IsNullOrEmpty(path))
                return "\\";
            return System.Web.HttpUtility.UrlDecode(System.IO.Path.Combine("\\", path.Replace('/', '\\')));
           
           
        }

        public override bool FileExists(string path)
        {
            return container.GetBlockBlobReference(NormalizePath(path)).Exists();
        }

        public override bool DirectoryExists(string path)
        {
            return container.GetBlockBlobReference(NormalizePath(path)+"/void").Exists();
        }

        public override void DeleteFile(string path)
        {
            container.GetBlockBlobReference(NormalizePath(path)).DeleteIfExists();
        }

        public override void DeleteDirectory(string path)
        {
            foreach (IListBlobItem item in container.ListBlobs(NormalizePath(path),true))
            {
                if (item.GetType() == typeof(CloudBlockBlob) || item.GetType().BaseType == typeof(CloudBlockBlob))
                {
                    ((CloudBlockBlob)item).DeleteIfExists();
                }
            }
        }

        public override System.IO.Stream OpenRead(string path)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(NormalizePath(path));
            if (blockBlob.Exists())
                return blockBlob.OpenRead();
            else
                return null;
        }

        public override System.IO.Stream OpenWrite(string path)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(NormalizePath(path));
            return blockBlob.OpenWrite();
        }

        public override void CreateSubDirectory(string path, string subDir)
        {
            CloudBlockBlob blockBlob=container.GetBlockBlobReference(NormalizePath(path)+"/"+subDir.ToLower() + "/void");
            if (!blockBlob.Exists()) 
                blockBlob.OpenWrite().Dispose();
        }

        public override List<FileSystem.ItemInfo> EnumerateDirectories(string path)
        {
            List<ItemInfo> result = new List<ItemInfo>();

            CloudBlobDirectory dir = container.GetDirectoryReference(NormalizePath(path));

            foreach (IListBlobItem item in dir.ListBlobs())
            {
                if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    CloudBlobDirectory directory = (CloudBlobDirectory)item;
                    CloudBlockBlob blob = directory.GetBlockBlobReference("void");
                    blob.FetchAttributes();

                    String dirName=directory.Uri.Segments[directory.Uri.Segments.Length-1];
                    if (dirName.EndsWith("/")) 
                        dirName=dirName.Substring(0,dirName.Length-1);
                    result.Add(new ItemInfo { Name = dirName, CreationTime = blob.Properties.LastModified.Value.DateTime, LastWriteTime = blob.Properties.LastModified.Value.DateTime, Length = 0 });
                }
            }
            return result;
        }

        public override List<FileSystem.ItemInfo> EnumerateFiles(string path, bool recurse = false)
        {
            List<ItemInfo> result = new List<ItemInfo>();
            CloudBlobDirectory topDir = container.GetDirectoryReference(NormalizePath(path));

            foreach (IListBlobItem item in topDir.ListBlobs(recurse))
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    blob.FetchAttributes();
                    if (!System.IO.Path.GetFileName(blob.Uri.AbsolutePath).Equals("void"))
                        result.Add(new ItemInfo { Name = recurse ? UnNormalizePath(blob.Uri.AbsolutePath.Replace(topDir.Uri.AbsolutePath,"")) :
                            blob.Uri.Segments[blob.Uri.Segments.Length - 1],
                                                  CreationTime = blob.Properties.LastModified.Value.DateTime,
                                                  LastWriteTime = blob.Properties.LastModified.Value.DateTime,
                                                  Length = blob.Properties.Length
                        });
                }
            }
            return result;
        }

        public override FileSystem.ItemInfo GetFileInfo(string path)
        {
            CloudBlockBlob blob = container.GetBlockBlobReference(NormalizePath(path));
            if (blob.Exists())
            {
                blob.FetchAttributes();
                return new ItemInfo { Name = blob.Uri.Segments[blob.Uri.Segments.Length - 1], CreationTime = blob.Properties.LastModified.Value.DateTime, LastWriteTime = blob.Properties.LastModified.Value.DateTime, Length = blob.Properties.Length };
            }
            else
                return null;
        }

        public override FileSystem.ItemInfo GetFileInfo2(string path)
        {
            CloudBlockBlob blob = container.GetBlockBlobReference(NormalizePath(path));
            if (blob.Exists())
            {
                blob.FetchAttributes();
                return new ItemInfo { Name = blob.Uri.Segments[blob.Uri.Segments.Length - 1], CreationTime = blob.Properties.LastModified.Value.DateTime, LastWriteTime = blob.Properties.LastModified.Value.DateTime, Length = blob.Properties.Length };
            }
            else
                return null;
        }

        public override FileSystem.ItemInfo GetDirectoryInfo(string path)
        {
            CloudBlockBlob blob = container.GetBlockBlobReference(NormalizePath(path)+"/void");

            if (String.IsNullOrEmpty(path))
            {
                return new ItemInfo { Name = "", CreationTime = DateTime.UtcNow, LastWriteTime = DateTime.UtcNow, Length = 0 };
            }
            else
            {
                CloudBlobDirectory directory = container.GetDirectoryReference(NormalizePath(path));
                if (blob.Exists())
                {
                    blob.FetchAttributes();
                    String dirName = directory.Uri.Segments[directory.Uri.Segments.Length - 1];
                    if (dirName.EndsWith("/"))
                        dirName = dirName.Substring(0, dirName.Length - 1);

                    return new ItemInfo { Name = dirName, CreationTime = blob.Properties.LastModified.Value.DateTime, LastWriteTime = blob.Properties.LastModified.Value.DateTime, Length = blob.Properties.Length };
                }
                else
                    return null;
            }

           
        }
    }
}
