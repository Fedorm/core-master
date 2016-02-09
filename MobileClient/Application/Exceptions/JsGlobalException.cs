using System;

namespace BitMobile.Application.Exceptions
{

    /// <summary>
    /// Exception, throwed in JS functions
    /// </summary>
    public class JsGlobalException : JsException
    {
        public JsGlobalException(string message)
            : base(message)
        {
        }

        public JsGlobalException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}