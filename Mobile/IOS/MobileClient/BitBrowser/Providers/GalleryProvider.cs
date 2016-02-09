using System;
using BitMobile.Common;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Runtime.InteropServices;
using BitMobile.Application;
using System.IO;
using BitMobile.Utilities.Translator;
using BitMobile.ClientModel;

namespace BitMobile.IOS
{
	public class GalleryProvider: ImagePickerProvider, IGalleryProvider
	{

		public GalleryProvider (UINavigationController controller, ApplicationContext context)
			:base(controller, context)
		{
		}

		#region IGalleryProvider implementation

		public void Copy (string path, int size, Action<object, Gallery.CallbackArgs> callback, object state)
		{
			WriteImage (path
				, size
				, result => callback (state, new Gallery.CallbackArgs (result))
				, UIImagePickerControllerSourceType.PhotoLibrary
				, UIImagePickerController.AvailableMediaTypes (UIImagePickerControllerSourceType.PhotoLibrary));
		}

		#endregion
	}
}

