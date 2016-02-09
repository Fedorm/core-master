using System;
using MonoTouch.Foundation;

namespace Microsoft.Synchronization.ClientServices
{
	public class NSUrlEventArgs: EventArgs
	{
		public string FilePath { get; private set; }

		public string Error { get; private set; }

		public NSUrlEventArgs (string filePath)
		{
			FilePath = filePath;
		}

		public NSUrlEventArgs (NSError error)
		{
			Error = string.Format ("NSUrlError: {0}; Description: {1}", error.ToString (), error.Description);
		}
	}
}

