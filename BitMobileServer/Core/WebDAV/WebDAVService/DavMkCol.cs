//-----------------------------------------------------------------------
// <copyright file="DavMkCol.cs" company="Sphorium Technologies">
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
	/// Custom implementation example for DavMKCol.
	/// </summary>
	public sealed class DavMKCol : DavMKColBase
	{
		public DavMKCol()
		{
			this.ProcessDavRequest += new DavProcessEventHandler(DavMKCol_ProcessDavRequest);
		}

        private VirtualDirectory Directory
        {
            get
            {
                return (VirtualDirectory)this.UserData;
            }
        }


		private void DavMKCol_ProcessDavRequest(object sender, EventArgs e)
		{

            if (HttpContext.Current == null)
                base.AbortRequest(ServerResponseCode.BadRequest);
            else
            {
                FileItem item = Directory.GetItem(VirtualDirectory.PathInfo(SolutionName));
                
                if (item==null) {
                    base.AbortRequest(ServerResponseCode.BadRequest);
                    return;
                }

                if (item.AccessRights.Equals("r"))
                {
                    base.AbortRequest(DavMKColResponseCode.MethodNotAllowed);
                    return;
                }

                //Check to see if the RequestPath is already a resource


                if (Directory._fileSystem.DirectoryExists(item.RelativePath))
                    base.AbortRequest(DavMKColResponseCode.MethodNotAllowed);
                else
                {

                    //The parent folder does not exist
                    if (!Directory._fileSystem.DirectoryExists(System.IO.Path.GetDirectoryName(item.RelativePath))) 
                        base.AbortRequest(DavMKColResponseCode.Conflict);
                    else
                    {
                        try
                        {
                            //Create the subFolder
                            Directory._fileSystem.CreateSubDirectory(System.IO.Path.GetDirectoryName(item.RelativePath),System.IO.Path.GetFileName(item.RelativePath));

                        }
                        catch (Exception)
                        {
                            base.AbortRequest(DavMKColResponseCode.Forbidden);
                        }
                    }
                }
            }
		}
	}
}
