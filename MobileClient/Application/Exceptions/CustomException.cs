using System;
using BitMobile.Application.Translator;

namespace BitMobile.Application.Exceptions
{
    public abstract class CustomException : Exception
    {
        string _specialReport;
        bool _hasSpecialReport;

        public virtual string FriendlyMessage
        {
            get { return D.APP_HAS_BEEN_INTERRUPTED; }
        }

        public virtual string Report
        {
            get { return _hasSpecialReport ? _specialReport : ToString(); }
            set
            {
                _specialReport = value;
                _hasSpecialReport = true;
            }
        }

        public abstract bool IsFatal { get; }

        public CustomException()
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