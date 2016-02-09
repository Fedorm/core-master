using System;
using MonoTouch.Foundation;
using Microsoft.Synchronization.Services.Formatters;

namespace Microsoft.Synchronization.ClientServices
{
	sealed class NSUrlDownloadDelegate: NSUrlSessionDownloadDelegate
	{
		EventHandler<NSUrlEventArgs> _downloadCompleted;
		Action<int, int> _progress;

		public NSUrlDownloadDelegate (EventHandler<NSUrlEventArgs> downloadCompleted, Action<int, int> progress)
		{
			_downloadCompleted = downloadCompleted;
			_progress = progress;
		}


		public override void DidWriteData (NSUrlSession session, NSUrlSessionDownloadTask downloadTask, 
			long bytesWritten, long totalBytesWritten, long totalBytesExpectedToWrite)
		{
			_progress((int)totalBytesExpectedToWrite, (int)totalBytesWritten);
		}

		// Called when the download task completes successfully.
		public override void DidFinishDownloading (NSUrlSession session, NSUrlSessionDownloadTask downloadTask, 
			NSUrl location)
		{
			_downloadCompleted(this, new NSUrlEventArgs(location.ToString()));
		}

		// This method is always called after the task completes.
		// If there is no error, the 'error' argument is null.
		public override void DidCompleteWithError (NSUrlSession session, NSUrlSessionTask task, NSError error)
		{
			if (error != null)
				_downloadCompleted(this, new NSUrlEventArgs(error));
		}

		// Called when all background events for the session are complete.
		public override void DidFinishEventsForBackgroundSession(NSUrlSession session)
		{
		}
	}
}

