using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BitMobile.ClientModel;
using BitMobile.Utilities.Exceptions;
using BitMobile.Utilities.Translator;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
	public class ImagePickerProvider
	{
		protected ApplicationContext _context;
		UINavigationController _controller;
		UIImagePickerController _imagePicker;
		UIAlertView _questionDialog;

		public ImagePickerProvider (UINavigationController controller, ApplicationContext context)
		{
			_controller = controller;
			_context = context;
		}

		protected void WriteImage (string path, int size, Action<bool> callback, UIImagePickerControllerSourceType sourceType, string[] mediaTypes)
		{
			Present (sourceType
				, mediaTypes
				, data => {
					if(data == null)
						callback(false);
					else
					{
						if (File.Exists (path)) {
							_questionDialog = new UIAlertView (D.WARNING, D.FILENAME_EXISTS_REPLACE, null, D.YES, D.NO);
							_questionDialog.Clicked += (object sender, UIButtonEventArgs e) => {
								if (e.ButtonIndex == 0) {
									try {
										File.Delete (path);
									} catch (IOException) {
										callback (false);
										return;
									}
									Save (path, size, callback, data);
								} else
									callback (false);
							};
							_questionDialog.Show ();
						} else {
							string dir = Path.GetDirectoryName (path);
							if (!Directory.Exists (dir))
								Directory.CreateDirectory (dir);
							Save (path, size, callback, data);
						}
					}
				});
		}

		void Present (UIImagePickerControllerSourceType sourceType, string[] mediaTypes, Action<NSDictionary> onPick)
		{
			_imagePicker = new UIImagePickerController ();
			_imagePicker.SourceType = sourceType;
			if (mediaTypes != null)
				_imagePicker.MediaTypes = mediaTypes;

			_imagePicker.Delegate = new ImagePickerDelegate (onPick);

			_controller.PresentViewController (_imagePicker, true, null);
		}

		void Save (string path, int size, Action<bool> callback, NSDictionary data)
		{
			bool result = false;

			UIImage photo = null;
			UIImage source = null;
			NSData file = null;
			NSError error = null;

			try {
				source = data.ValueForKey (new NSString ("UIImagePickerControllerOriginalImage")) as UIImage;
				if (source != null) {

					photo = ScaleImage (source, size);
					file = photo.AsJPEG ();
					error = null;
					bool saved = file.Save (path, false, out error);
					if (!saved)
						_context.HandleException (new NonFatalException (D.IO_EXCEPTION, error.LocalizedDescription));
					result = saved;
				}
			} finally {
				if (photo != null)
					photo.Dispose ();
				if (source != null)
					source.Dispose ();
				if (file != null)
					file.Dispose ();
				if (error != null)
					error.Dispose ();
			}

			Task.Run (() => {
				Thread.Sleep (300);
				_context.InvokeOnMainThread (() => callback (result));
			});
		}

		static UIImage ScaleImage (UIImage image, int maxSize)
		{
			UIImage res;

			using (CGImage imageRef = image.CGImage) {
				CGImageAlphaInfo alphaInfo = imageRef.AlphaInfo;
				CGColorSpace colorSpaceInfo = CGColorSpace.CreateDeviceRGB ();
				if (alphaInfo == CGImageAlphaInfo.None) {
					alphaInfo = CGImageAlphaInfo.NoneSkipLast;
				}

				int width, height;

				width = imageRef.Width;
				height = imageRef.Height;


				if (height >= width) {
					width = (int)Math.Floor ((double)width * ((double)maxSize / (double)height));
					height = maxSize;
				} else {
					height = (int)Math.Floor ((double)height * ((double)maxSize / (double)width));
					width = maxSize;
				}


				CGBitmapContext bitmap;

				int bytesPerRow = (int)image.Size.Width * 4;
				byte[] buffer = new byte[(int)(bytesPerRow * image.Size.Height)];

				if (image.Orientation == UIImageOrientation.Up || image.Orientation == UIImageOrientation.Down) {
					bitmap = new CGBitmapContext (buffer, width, height, imageRef.BitsPerComponent, imageRef.BytesPerRow, colorSpaceInfo, alphaInfo);
				} else {
					bitmap = new CGBitmapContext (buffer, height, width, imageRef.BitsPerComponent, imageRef.BytesPerRow, colorSpaceInfo, alphaInfo);
				}

				switch (image.Orientation) {
				case UIImageOrientation.Left:
					bitmap.RotateCTM ((float)Math.PI / 2);
					bitmap.TranslateCTM (0, -height);
					break;
				case UIImageOrientation.Right:
					bitmap.RotateCTM (-((float)Math.PI / 2));
					bitmap.TranslateCTM (-width, 0);
					break;
				case UIImageOrientation.Up:
					break;
				case UIImageOrientation.Down:
					bitmap.TranslateCTM (width, height);
					bitmap.RotateCTM (-(float)Math.PI);
					break;
				}

				bitmap.DrawImage (new RectangleF (0, 0, width, height), imageRef);


				res = UIImage.FromImage (bitmap.ToImage ());
				bitmap = null;

			}

			return res;
		}

		class ImagePickerDelegate : UIImagePickerControllerDelegate
		{
			Action<NSDictionary> _onPick;

			public ImagePickerDelegate (Action<NSDictionary> onPick)
			{
				_onPick = onPick;
			}

			public override void FinishedPickingMedia (UIImagePickerController picker, NSDictionary info)
			{
				_onPick (info);
				picker.DismissViewController (true, null);
				picker.Dispose ();
			}

			public override void Canceled (UIImagePickerController picker)
			{
				_onPick (null);
				picker.DismissViewController (true, null);
				picker.Dispose ();
			}
		}
	}
}

