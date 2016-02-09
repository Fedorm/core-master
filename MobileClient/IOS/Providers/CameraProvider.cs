using System;
using BitMobile.Application.Translator;
using BitMobile.Common.Device.Providers;
using MonoTouch.AVFoundation;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
    public class CameraProvider : ImagePickerProvider, ICameraProvider
    {
        public CameraProvider(UINavigationController controller, IOSApplicationContext context)
            : base(controller, context)
        {
        }

        #region ICameraProvider implementation

        public void MakeSnapshot(string path, int size, Action<bool> callback)
        {
            AVAuthorizationStatus status = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);

            switch (status)
            {
                case AVAuthorizationStatus.NotDetermined:
                    AVCaptureDevice.RequestAccessForMediaType(AVMediaType.Video, granded =>
                    {
                        if (granded)
                            Context.MainController.InvokeOnMainThread(() => Write(path, size, callback));
                    });
                    break;
                case AVAuthorizationStatus.Restricted:
                case AVAuthorizationStatus.Denied:
                    using (var alert = new UIAlertView(D.WARNING, D.CAMERA_NOT_ALLOWED, null, D.OK))
                        alert.Show();
                    break;
                case AVAuthorizationStatus.Authorized:
                    Write(path, size, callback);
                    break;
            }
        }

        #endregion

        private void Write(string path, int size, Action<bool> callback)
        {
            WriteImage(path
                , size
                , result => callback(result)
                , UIImagePickerControllerSourceType.Camera
                , UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary));
        }
    }
}