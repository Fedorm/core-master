using BitMobile.Application.Translator;

namespace BitMobile.Application.Exceptions
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