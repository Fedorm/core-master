//-----------------------------------------------------------------------
// <copyright file="DavPropFind.cs" company="Sphorium Technologies">
//     Copyright (c) Sphorium Technologies. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Web;
using Sphorium.WebDAV.Server.Framework.BaseClasses;
using Sphorium.WebDAV.Server.Framework.Classes;
using Sphorium.WebDAV.Server.Framework.Collections;
using Sphorium.WebDAV.Server.Framework.Resources;

namespace BMWebDAV
{
	/// <summary>
	/// Custom implementation example for DavPropFind.
	/// </summary>
	public sealed class DavPropFind : DavPropFindBase
	{
		public DavPropFind()
		{
			this.ProcessDavRequest += new DavProcessEventHandler(DavPropFind_ProcessDavRequest);
		}

        private VirtualDirectory Directory
        {
            get
            {
                return (VirtualDirectory)this.UserData;
            }
        }

		private void DavPropFind_ProcessDavRequest(object sender, EventArgs e)
		{
            if (HttpContext.Current == null)
                base.AbortRequest(ServerResponseCode.BadRequest);
            else
            {

                String _path = VirtualDirectory.PathInfo(SolutionName);
                String _basePath = HttpContext.Current.Request.Url.AbsoluteUri;

                if (!Path.HasExtension(_basePath) && !_basePath.EndsWith("/"))
                    _basePath += "/";

                FileItem item = Directory.GetItem(_path);

                
                if ((!string.IsNullOrEmpty(_path)) && (!_path.Equals("/")) && (item == null))  // no such item
                {
                    base.AbortRequest(ServerResponseCode.NotFound);
                    return;
                }


                if (item!=null) 
                {
                    if ((item is VirtualFile) || (Directory._fileSystem.FileExists(item.RelativePath))) // file
                    {

                        if (item is VirtualFile)
                        {
                            DavFile _davFile = new DavFile(item.Name, _basePath);
                            _davFile.CreationDate = DateTime.Now.ToUniversalTime();
                            _davFile.LastModified = DateTime.Now.ToUniversalTime();

                            _davFile.SupportsExclusiveLock = true;
                            _davFile.SupportsSharedLock = true;
                            _davFile.ContentLength = 0;

                            base.FileResources.Add(_davFile);
                            return;
                        }

                        FileSystem.ItemInfo _fileInfo = Directory._fileSystem.GetFileInfo(item.RelativePath);

                        if (_fileInfo != null)
                        {

                            DavFile _davFile = new DavFile(_fileInfo.Name, _basePath);
                            _davFile.CreationDate = _fileInfo.CreationTime;
                            _davFile.LastModified = _fileInfo.LastWriteTime;

                            _davFile.SupportsExclusiveLock = true;
                            _davFile.SupportsSharedLock = true;
                            _davFile.ContentLength = (int)_fileInfo.Length;

                            base.FileResources.Add(_davFile);
                            return;
                        }
                    }
                }
                //dir
                DavFolder _rootResource;
                FileSystem.ItemInfo _dirInfo;
                if (item == null)
                {
                    _rootResource = new DavFolder("DavWWWRoot", _basePath);
                    _dirInfo = Directory._fileSystem.GetDirectoryInfo("");
                }
                else
                {
                    _dirInfo = Directory._fileSystem.GetDirectoryInfo(item.RelativePath);
                    _rootResource = new DavFolder(_dirInfo.Name, _basePath);
                }
                _rootResource.CreationDate = _dirInfo.CreationTime;
                _rootResource.LastModified = _dirInfo.LastWriteTime;
                _rootResource.ContentLength = (int)_dirInfo.Length;

                base.CollectionResources.Add(_rootResource);


                    //TODO: Only populate the requested properties
                switch (base.RequestDepth)
                {
                    case Sphorium.WebDAV.Server.Framework.DepthType.ResourceOnly:
                        break;

                    default:

                        if (item == null) //root dir
                        {

                            foreach (FileItem subItem in Directory.items)
                            {
                                if (subItem is VirtualFile)
                                {
                                    DavFile _davFile = new DavFile(subItem.Name, _basePath + subItem.Name);
                                    _davFile.CreationDate = DateTime.Now.ToUniversalTime();
                                    _davFile.LastModified = DateTime.Now.ToUniversalTime();

                                    _davFile.SupportsExclusiveLock = true;
                                    _davFile.SupportsSharedLock = true;
                                    _davFile.ContentLength = 0;

                                    base.FileResources.Add(_davFile);
                                }
                                else
                                {
                                    DavFolder _davFolder = new DavFolder(subItem.Name, _basePath + subItem.Name);
                                    FileSystem.ItemInfo _subDir = Directory._fileSystem.GetDirectoryInfo(subItem.RelativePath);

                                    _davFolder.CreationDate = _subDir.CreationTime;
                                    _davFolder.LastModified = _subDir.LastWriteTime;
                                    _davFolder.ContentLength =  (int)_subDir.Length;
                                    base.CollectionResources.Add(_davFolder);
                                }
                            }
                        }
                        else // non root dir
                        {
                            foreach (FileSystem.ItemInfo _subDir in Directory._fileSystem.EnumerateDirectories(item.RelativePath))
                            {
                                //TODO: Only populate the requested properties
                                DavFolder _davFolder = new DavFolder(_subDir.Name, _basePath + _subDir.Name);
                                _davFolder.CreationDate = _subDir.CreationTime;
                                _davFolder.LastModified = _subDir.LastWriteTime;
                                _davFolder.ContentLength = (int)_subDir.Length;

                                base.CollectionResources.Add(_davFolder);
                            }

                            foreach (FileSystem.ItemInfo _fileInfo in Directory._fileSystem.EnumerateFiles(item.RelativePath))
                            {
                                //TODO: Only populate the requested properties
                                DavFile _davFile = new DavFile(_fileInfo.Name, _basePath +  _fileInfo.Name);
                                _davFile.CreationDate = _fileInfo.CreationTime;
                                _davFile.LastModified = _fileInfo.LastWriteTime;

                                _davFile.SupportsExclusiveLock = true;
                                _davFile.SupportsSharedLock = true;
                                _davFile.ContentLength = (int)_fileInfo.Length;



                                base.FileResources.Add(_davFile);
                            }
                        }
                        break;
                    }
                }
		}
	}
}
