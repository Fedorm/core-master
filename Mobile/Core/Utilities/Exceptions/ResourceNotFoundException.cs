using System;
using BitMobile.Utilities.Translator;

namespace BitMobile.Utilities.Exceptions
{
	public class ResourceNotFoundException : CustomException
	{
		public string ResourceType { get; private set; }

		public string ResourceName { get; private set; }

		public override string FriendlyMessage {
			get { return D.LOADING_RESOURCE_ERROR; }
		}

		public override bool IsFatal {
			get { return true; }
		}

		public ResourceNotFoundException (String resType, String resName)
            : base(BuildMessage(resType, resName))
		{
			ResourceType = resType;
			ResourceName = resName;
		}

		public ResourceNotFoundException (String resType, String resName, Exception innerException)
            :base(BuildMessage(resType, resName), innerException)
		{
			ResourceType = resType;
			ResourceName = resName;
		}

		static string BuildMessage (String resType, String resName)
		{
			return string.Format ("Resource {0} of type {1} not found", resName, resType);
		}
	}
}