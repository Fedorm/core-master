using System;
using BitMobile.Application.Translator;

namespace BitMobile.Application.Exceptions
{
    public class InternalServerException : ConnectionException
    {
        readonly string _friendlyMessage = D.INTERNAL_SERVER_ERROR;

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