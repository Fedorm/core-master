using System;

namespace BitMobile.Application.Exceptions
{
    public class NonFatalException : CustomException
    {
        readonly string _friendlyMessage;

        public override bool IsFatal { get { return false; } }

        public override string FriendlyMessage
        {
            get
            {
                return _friendlyMessage;
            }
        }

        public NonFatalException(string friendlyMessage)
            : base(friendlyMessage)
        {
            _friendlyMessage = friendlyMessage;
        }

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
