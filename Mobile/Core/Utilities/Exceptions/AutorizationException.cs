using BitMobile.Utilities.Translator;
using System;

namespace BitMobile.Utilities.Exceptions
{
	public class AutorizationException : InternalServerException
	{
		public override string FriendlyMessage {
			get { return D.AUTORIZATION_ERROR; }
		}

		public override string Report {
            get { return D.AUTORIZATION_ERROR; }
		}

		public override bool IsFatal {
			get { return false; }
		}

		public AutorizationException (string message)
            : base(message)
		{
		}
	}
}