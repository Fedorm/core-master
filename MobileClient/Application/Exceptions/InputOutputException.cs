using System;
using BitMobile.Application.Translator;

namespace BitMobile.Application.Exceptions
{
    public class InputOutputException : CustomException
    {
        public override string FriendlyMessage
        {
            get
            {
                return D.IO_EXCEPTION;
            }
        }

        public override bool IsFatal { get { return false; } }

        public InputOutputException(Exception innerException, string message, params object[] args)
            : base(string.Format(message, args), innerException)
        {
        }
    }
}
