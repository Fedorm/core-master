using BitMobile.Utilities.Translator;

namespace BitMobile.Utilities.Exceptions
{
    public class LicenseException : InternalServerException
	{
		public override bool IsFatal {
			get { return false; }
		}

		public override string FriendlyMessage {
			get { return D.LICENSE_ERROR; }
		}

		public override string Report {
			get { return null; }
		}
	}
}
