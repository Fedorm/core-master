using System;
using BitMobile.Common.Device.Providers;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
    public class GalleryProvider : ImagePickerProvider, IGalleryProvider
    {
        public GalleryProvider(UINavigationController controller, IOSApplicationContext context)
            : base(controller, context)
        {
        }

        #region IGalleryProvider implementation

        public void Copy(string path, int size, Action<bool> callback)
        {
            WriteImage(path
                , size
                , result => callback(result)
                , UIImagePickerControllerSourceType.PhotoLibrary
                , UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary));
        }

        #endregion
    }
}