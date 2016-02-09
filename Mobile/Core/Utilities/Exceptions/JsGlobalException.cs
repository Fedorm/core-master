using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitMobile.Utilities.Exceptions
{

    /// <summary>
    /// Exception, throwed in JS functions
    /// </summary>
    public class JsGlobalException : JSException
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