//-----------------------------------------------------------------------
// <copyright file="DavDelete.cs" company="Sphorium Technologies">
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
	/// Custom implementation example for DavDeleteBase
	/// </summary>
	public sealed class DavDelete : DavDeleteBase
	{
		public DavDelete()
		{
			this.ProcessDavRequest += new DavProcessEventHandler(DavDelete_ProcessDavRequest);
		}

        private VirtualDirectory Directory
        {
            get
            {
                return (VirtualDirectory)this.UserData;
            }
        }

		private void DavDelete_ProcessDavRequest(object sender, EventArgs e)
		{
            if (HttpContext.Current == null)
                base.AbortRequest(ServerResponseCode.BadRequest);
            else
            {

                //Check to see if the resource is a folder

                FileItem item = Directory.GetItem(VirtualDirectory.PathInfo(SolutionName));
                if (item==null) {
                    base.AbortRequest(ServerResponseCode.BadRequest);
                    return;
                }
                    
                if (item.AccessRights.Equals("r"))
                {
                    base.AbortRequest(403);
                    return;
                }

                if (Directory._fileSystem.DirectoryExists(item.RelativePath)) {
                    try
                    {
                        Directory._fileSystem.DeleteDirectory(item.RelativePath);
                    }
                    catch (Exception) {
                        base.AbortRequest(DavDeleteResponseCode.Locked);
                        return;
                    }
                }
                else
                {
                        try
                        {
                            Directory._fileSystem.DeleteFile(item.RelativePath);
                        }
                        catch (Exception)
                        {
                            base.AbortRequest(DavDeleteResponseCode.Locked);
                            return;
                        }
                }
            }
		}
	}
}
