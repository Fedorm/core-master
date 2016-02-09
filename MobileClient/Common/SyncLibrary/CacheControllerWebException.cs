using System;
using System.Net;

namespace BitMobile.Common.SyncLibrary
{
    public class CacheControllerWebException : Exception
    {
        public CacheControllerWebException(string message, HttpStatusCode statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; private set; }
    }
}
