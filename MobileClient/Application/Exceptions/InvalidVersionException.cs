using BitMobile.Application.Translator;

namespace BitMobile.Application.Exceptions
{
    public class InvalidVersionException : InternalServerException
	{
		public override string FriendlyMessage {
			get {
				return D.UNSUPPORTED_PLATFORM;
			}
		}

		public override bool IsFatal {
			get { return false; }
		}

		public InvalidVersionException (string message)
            : base(message)
		{
		}
	}
}
