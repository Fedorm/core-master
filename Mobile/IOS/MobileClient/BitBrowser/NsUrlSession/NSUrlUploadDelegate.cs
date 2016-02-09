using System;
using MonoTouch.Foundation;
using BitMobile.DataAccessLayer;
using Microsoft.Synchronization.Services.Formatters;

namespace Microsoft.Synchronization.ClientServices
{
	public class NSUrlUploadDelegate: NSUrlSessionDownloadDelegate
	{
		EventHandler<NSUrlEventArgs> _uploadCompleted;
		OnStatus _progress;

		public NSUrlUploadDelegate (EventHandler<NSUrlEventArgs> uploadCompleted, OnStatus progress)
		{
			_uploadCompleted = uploadCompleted;
			_progress = progress;
		}

		public override void DidSendBodyData (NSUrlSession session, NSUrlSessionTask task, long bytesSent, 
		                                      long totalBytesSent, long totalBytesExpectedToSend)
		{
			_progress ((int)totalBytesExpectedToSend, (int)totalBytesSent);
		}

		public override void DidFinishDownloading (NSUrlSession session, NSUrlSessionDownloadTask downloadTask, 
		                                           NSUrl location)
		{
			_uploadCompleted (this, new NSUrlEventArgs (location.ToString ()));
		}

		public override void DidCompleteWithError (NSUrlSession session, NSUrlSessionTask task, NSError error)
		{
			if (error != null) {
				_uploadCompleted (this, new NSUrlEventArgs (error));
			}
		}

		public override void DidFinishEventsForBackgroundSession (NSUrlSession session)
		{
			Console.WriteLine ("DidFinishEventsForBackgroundSession");
		}
	}
}

