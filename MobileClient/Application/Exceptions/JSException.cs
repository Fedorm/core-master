using System;

namespace BitMobile.Application.Exceptions
{
    public class JsException : CustomException
    {
        public override bool IsFatal
        { get { return true; } }

        public JsException(string message)
            : base(message)
        {
        }

        public JsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}