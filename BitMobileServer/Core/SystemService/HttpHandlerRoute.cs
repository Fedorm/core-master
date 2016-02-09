using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace SystemService
{
    public class HttpHandlerRoute : IRouteHandler
    {
        private String _solution = null;

        public HttpHandlerRoute(String solution)
        {
            _solution = solution;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            IHttpHandler httpHandler = new BMWebDAV.BMWebDAVHandler(_solution);
            return httpHandler;
        }
    }
}
