using System;
using BitMobile.Utilities.Translator;

namespace BitMobile.Utilities.Exceptions
{
    public class InternalServerException : ConnectionException
    {
        string _friendlyMessage = D.INTERNAL_SERVER_ERROR;

        public override string FriendlyMessage
        {
            get { return _friendlyMessage; }
        }

        public override bool IsFatal
        {
            get { return false; }
        }

        public InternalServerException()
            : base()
        {
        }

        public InternalServerException(string message)
            : base(message)
        {
        }

        public InternalServerException(string message, string friendlyMessage)
            : base(message)
        {
            _friendlyMessage = friendlyMessage;
        }

        public InternalServerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InternalServerException(string message, Exception innerException, string friendlyMessage)
            : base(message, innerException)
        {
            _friendlyMessage = friendlyMessage;
        }
    }
}