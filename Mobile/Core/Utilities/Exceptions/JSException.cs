using System;

namespace BitMobile.Utilities.Exceptions
{
    public class JSException : CustomException
    {
        public override bool IsFatal
        { get { return true; } }

        public JSException(string message)
            : base(message)
        {
        }

        public JSException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}