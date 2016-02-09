using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Utilities.Exceptions
{
    public class NonFatalException : CustomException
    {
        public override bool IsFatal { get { return false; } }

        public override string FriendlyMessage
        {
            get
            {
                return _friendlyMessage;
            }
        }
        string _friendlyMessage;

        public NonFatalException(string friendlyMessage, string message)
            : base(message)
        {
            _friendlyMessage = friendlyMessage;
        }

        public NonFatalException(string friendlyMessage, string message, Exception innerException)
            : base(message, innerException)
        {
            _friendlyMessage = friendlyMessage;
        }
    }
}
