//-----------------------------------------------------------------------
// <copyright file="DavGet.cs" company="Sphorium Technologies">
//     Copyright (c) Sphorium Technologies. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Web;

using Sphorium.WebDAV.Server.Framework.BaseClasses;

namespace BMWebDAV
{
	/// <summary>
	/// Custom implementation example for DavGet.
	/// </summary>
	public sealed class DavGet : DavGetBase
	{
		public DavGet()
		{
			this.ProcessDavRequest += new DavProcessEventHandler(DavGet_ProcessDavRequest);
		}

        private VirtualDirectory Directory
        {
            get
            {
                return (VirtualDirectory)this.UserData;
            }
        }


		private void DavGet_ProcessDavRequest(object sender, EventArgs e)
		{
            if (HttpContext.Current == null)
                base.AbortRequest(ServerResponseCode.BadRequest);
            else
            {
                FileItem item = Directory.GetItem(VirtualDirectory.PathInfo(SolutionName));

                if (item == null) 
                    base.AbortRequest(ServerResponseCode.BadRequest);
                else
                {
                    if (item.RelativePath.EndsWith(@"\mkdir"))
                    {
                        String[] arr = item.RelativePath.Split(new String[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                        String folderToCreate = arr[arr.Length - 2].Trim();
                        if (!String.IsNullOrEmpty(folderToCreate))
                        {
                            String rp = "";
                            for (int i = 0; i < arr.Length - 2; i++)
                            {
                                rp = rp + arr[i].Trim() + "\\";
                            }

                            if (!Directory._fileSystem.DirectoryExists(rp + folderToCreate))
                            {
                                Directory._fileSystem.CreateSubDirectory(rp, folderToCreate);
                            }
                        }

                        base.ResponseOutput = System.Text.ASCIIEncoding.ASCII.GetBytes("ok");
                        return;
                    }

                    if (item is VirtualFile)
                    {
                        ((VirtualFile)item).GetData(HttpContext.Current.Response.OutputStream);
                        base.ResponseOutput = new byte[0];
                    }
                    else
                    {
                        if (!Directory._fileSystem.FileExists(item.RelativePath)) 
                            base.AbortRequest(ServerResponseCode.NotFound);
                        else
                        {
                            using (Stream _fileStream = Directory._fileSystem.OpenRead(item.RelativePath))
                            {
                                Stream output = ImageResizer.TryResize(Directory._fileSystem.GetFileInfo2(item.RelativePath).Name, _fileStream, HttpContext.Current.Request.QueryString);

                                long _fileSize = output.Length;
                                byte[] _responseBytes = new byte[_fileSize];

                                output.Read(_responseBytes, 0, (int)_fileSize);

                                base.ResponseOutput = _responseBytes;

                                output.Close();
                            }
                        }
                    }
                }
            }
		}
	}
}
