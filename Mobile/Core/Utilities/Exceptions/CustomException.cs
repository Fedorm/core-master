using System;
using BitMobile.Utilities.Translator;

namespace BitMobile.Utilities.Exceptions
{
    public abstract class CustomException : Exception
    {
        string _specialReport = null;
        bool _hasSpecialReport = false;

        public virtual string FriendlyMessage
        {
            get { return D.APP_HAS_BEEN_INTERRUPTED; }
        }

        public virtual string Report
        {
            get { return _hasSpecialReport ? _specialReport : this.ToString(); }
            set
            {
                _specialReport = value;
                _hasSpecialReport = true;
            }
        }

        public abstract bool IsFatal { get; }

        public CustomException()
            : base()
        {
        }

        public CustomException(string message)
            : base(message)
        {
        }

        public CustomException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}