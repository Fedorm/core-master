using BitMobile.Utilities.Translator;
using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Utilities.Exceptions
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
