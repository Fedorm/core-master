using System;
using BitMobile.Utilities.Translator;

namespace BitMobile.Utilities.Exceptions
{
	public class ConnectionException : CustomException
	{
		public override string FriendlyMessage {
			get { return D.CONNECTION_EXCEPTION; }
		}

		public override bool IsFatal {
			get { return false; }
		}

		public ConnectionException ()
            : base()
		{
		}

		public ConnectionException (string message)
            : base(message)
		{
		}

		public ConnectionException (string message, Exception innerException)
            : base(message, innerException)
		{
		}
	}
}