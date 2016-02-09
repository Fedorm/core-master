//-----------------------------------------------------------------------
// <copyright file="DavPut.cs" company="Sphorium Technologies">
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
	/// Custom implementation example for DavPut.
	/// </summary>
	public sealed class DavPut : DavPutBase
	{
		public DavPut()
		{
			this.ProcessDavRequest += new DavProcessEventHandler(DavPut_ProcessDavRequest);
		}

        private VirtualDirectory Directory
        {
            get
            {
                return (VirtualDirectory)this.UserData;
            }
        }


        private void DavPut_ProcessDavRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current == null)
                base.AbortRequest(ServerResponseCode.BadRequest);
            else
            {
                FileItem item = Directory.GetItem(VirtualDirectory.PathInfo(SolutionName));
                if (item == null)
                {
                    base.AbortRequest(ServerResponseCode.BadRequest);
                    return;
                }

                if (item.AccessRights.Equals("r"))
                {
                    base.AbortRequest(ServerResponseCode.BadRequest);
                    return;
                }

                if (item.RelativePath.EndsWith(@"\mkdir"))
                {
                    String[] arr = item.RelativePath.Split(new String[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                    String folderToCreate = arr[arr.Length-2].Trim();
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
                    return;
                }

                //The parent folder does not exist
                if (!Directory._fileSystem.DirectoryExists(System.IO.Path.GetDirectoryName(item.RelativePath)))
                    base.AbortRequest(ServerResponseCode.NotFound);
                else
                {
                    if (!base.OverwriteExistingResource)
                    {
                        //Check to see if the resource already exists
                        if (System.IO.File.Exists(item.RelativePath))
                            base.AbortRequest(DavPutResponseCode.Conflict);
                        else
                        {
                            SaveFile(item);
                        }
                    }
                    else
                    {
                        SaveFile(item);
                    }
                }


            }
        }

		private void SaveFile(FileItem item)
		{
			byte[] _requestInput = base.GetRequestInput();
            using (Stream _newFile = Directory._fileSystem.OpenWrite(item.RelativePath))
			{
				_newFile.Write(_requestInput, 0, _requestInput.Length);
                _newFile.Flush();
				_newFile.Close();
			}
		}
	}
}
