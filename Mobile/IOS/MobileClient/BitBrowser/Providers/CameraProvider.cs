using System;
using MonoTouch;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using BitMobile.Common;
using BitMobile.ClientModel;
using MonoTouch.AVFoundation;
using BitMobile.Utilities.Translator;

namespace BitMobile.IOS
{
	public class CameraProvider : ImagePickerProvider, ICameraProvider
	{
		public CameraProvider (UINavigationController controller, ApplicationContext context)
			: base (controller, context)
		{
		}

		#region ICameraProvider implementation

		public void MakeSnapshot (string path, int size, Action<object, Camera.CallbackArgs> callback, object state)
		{
			AVAuthorizationStatus status = AVCaptureDevice.GetAuthorizationStatus (AVMediaType.Video);

			switch (status) {
			case AVAuthorizationStatus.NotDetermined:
				AVCaptureDevice.RequestAccessForMediaType (AVMediaType.Video, granded => {
					if (granded)
						_context.MainController.InvokeOnMainThread (() => Write (path, size, callback, state));					
				} );
				break;
			case AVAuthorizationStatus.Restricted:
			case AVAuthorizationStatus.Denied:
				using (UIAlertView alert = new UIAlertView (D.WARNING, D.CAMERA_NOT_ALLOWED, null, D.OK))
					alert.Show ();
				break;
			case AVAuthorizationStatus.Authorized:
				Write (path, size, callback, state);						
				break;
			}
		}

		#endregion

		private void Write (string path, int size, Action<object, Camera.CallbackArgs> callback, object state) 		{ 			WriteImage (path
				, size
				, result => callback (state, new Camera.CallbackArgs (result))
				, UIImagePickerControllerSourceType.Camera
				, UIImagePickerController.AvailableMediaTypes (UIImagePickerControllerSourceType.PhotoLibrary)); 		}
	}
}

