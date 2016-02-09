using System.IO;
using System.Web;
using Sphorium.WebDAV.Server.Framework;
using System;

namespace BMWebDAV
{
	public class BMWebDAVHandler : IHttpHandler
	{
		private WebDavProcessor __webDavProcessor = new WebDavProcessor();
        private string _solution;

        public BMWebDAVHandler(string solution)
        {
            _solution = solution;
        }

		public bool IsReusable
		{
			get
			{
				return true;
			}
		}

		public void ProcessRequest(HttpContext context)
		{
            try
            {
                __webDavProcessor.ProcessRequest(context.ApplicationInstance);
            }
            catch (Exception e)
            {
                Common.Solution.LogException(_solution, "admin", e);
                throw;
            }
		}
	}
}
