using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BitMobile.Application.Exceptions;
using BitMobile.Application.IO;
using BitMobile.Application.Translator;
using BitMobile.Common.IO;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
    public class ImagePickerProvider
    {
        protected readonly IOSApplicationContext Context;
        private readonly UINavigationController _controller;
        private UIImagePickerController _imagePicker;
        private UIAlertView _questionDialog;

        public ImagePickerProvider(UINavigationController controller, IOSApplicationContext context)
        {
            _controller = controller;
            Context = context;
        }

        protected void WriteImage(string path, int size, Action<bool> callback,
            UIImagePickerControllerSourceType sourceType, string[] mediaTypes)
        {
            Present(sourceType
                , mediaTypes
                , data =>
                {
                    if (data == null)
                        callback(false);
                    else
                    {
                        if (IOContext.Current.Exists(path, FileSystemItem.File))
                        {
                            _questionDialog = new UIAlertView(D.WARNING, D.FILENAME_EXISTS_REPLACE, null, D.YES, D.NO);
                            _questionDialog.Clicked += (sender, e) =>
                            {
                                if (e.ButtonIndex == 0)
                                {
                                    try
                                    {
                                        IOContext.Current.Delete(path);
                                    }
                                    catch (IOException)
                                    {
                                        callback(false);
                                        return;
                                    }
                                    Save(path, size, callback, data);
                                }
                                else
                                    callback(false);
                            };
                            _questionDialog.Show();
                        }
                        else
                        {
                            string dir = Path.GetDirectoryName(path);
                            IOContext.Current.CreateDirectory(dir);
                            Save(path, size, callback, data);
                        }
                    }
                });
        }

        private void Present(UIImagePickerControllerSourceType sourceType, string[] mediaTypes,
            Action<NSDictionary> onPick)
        {
            _imagePicker = new UIImagePickerController();
            _imagePicker.SourceType = sourceType;
            if (mediaTypes != null)
                _imagePicker.MediaTypes = mediaTypes;

            _imagePicker.Delegate = new ImagePickerDelegate(onPick);

            _controller.PresentViewController(_imagePicker, false, null);
        }

        private void Save(string path, int size, Action<bool> callback, NSDictionary data)
        {
            bool result = false;

            UIImage photo = null;
            UIImage source = null;
            NSData file = null;
            NSError error = null;

            try
            {
                source = data.ObjectForKey(new NSString("UIImagePickerControllerOriginalImage")) as UIImage;
                if (source != null)
                {
                    photo = ScaleImage(source, size);
                    file = photo.AsJPEG();
                    bool saved = file.Save(path, false, out error);
                    if (!saved)
                        Context.HandleException(new NonFatalException(D.IO_EXCEPTION, error.LocalizedDescription));
                    result = saved;
                }
            }
            finally
            {
                if (photo != null)
                    photo.Dispose();
                if (source != null)
                    source.Dispose();
                if (file != null)
                    file.Dispose();
                if (error != null)
                    error.Dispose();
                if (data != null)
                    data.Dispose();
            }

            Task.Run(() =>
            {
                Thread.Sleep(300);
                Context.InvokeOnMainThread(() => callback(result));
            });
        }

        private static UIImage ScaleImage(UIImage image, int maxSize)
        {
            UIImage res = image;

            CGImage imageRef = image.CGImage;

            CGImageAlphaInfo alphaInfo = imageRef.AlphaInfo;
            CGColorSpace colorSpaceInfo = CGColorSpace.CreateDeviceRGB();
            if (alphaInfo == CGImageAlphaInfo.None)
            {
                alphaInfo = CGImageAlphaInfo.NoneSkipLast;
            }

            int width = imageRef.Width;
            int height = imageRef.Height;

            if (maxSize > 0 && maxSize < Math.Max(width, height))
            {
                try
                {
                    if (height >= width)
                    {
                        width = (int) Math.Floor(width*(maxSize/(double) height));
                        height = maxSize;
                    }
                    else
                    {
                        height = (int) Math.Floor(height*(maxSize/(double) width));
                        width = maxSize;
                    }

                    int bytesPerRow = (int) image.Size.Width*4;
                    var buffer = new byte[(int) (bytesPerRow*image.Size.Height)];

                    CGBitmapContext bitmap;
                    if (image.Orientation == UIImageOrientation.Up || image.Orientation == UIImageOrientation.Down)
                        bitmap = new CGBitmapContext(buffer, width, height, imageRef.BitsPerComponent,
                            imageRef.BytesPerRow, colorSpaceInfo, alphaInfo);
                    else
                        bitmap = new CGBitmapContext(buffer, height, width, imageRef.BitsPerComponent,
                            imageRef.BytesPerRow, colorSpaceInfo, alphaInfo);

                    switch (image.Orientation)
                    {
                        case UIImageOrientation.Left:
                            bitmap.RotateCTM((float) Math.PI/2);
                            bitmap.TranslateCTM(0, -height);
                            break;
                        case UIImageOrientation.Right:
                            bitmap.RotateCTM(-((float) Math.PI/2));
                            bitmap.TranslateCTM(-width, 0);
                            break;
                        case UIImageOrientation.Up:
                            break;
                        case UIImageOrientation.Down:
                            bitmap.TranslateCTM(width, height);
                            bitmap.RotateCTM(-(float) Math.PI);
                            break;
                    }

                    bitmap.DrawImage(new RectangleF(0, 0, width, height), imageRef);
                    res = UIImage.FromImage(bitmap.ToImage());
                }
                finally
                {
                    image.Dispose();
                }
            }


            return res;
        }

        private class ImagePickerDelegate : UIImagePickerControllerDelegate
        {
            private readonly Action<NSDictionary> _onPick;

            public ImagePickerDelegate(Action<NSDictionary> onPick)
            {
                _onPick = onPick;
            }

            public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info)
            {
                _onPick(info);
                picker.DismissViewController(false, null);
                picker.Dispose();
            }

            public override void Canceled(UIImagePickerController picker)
            {
                _onPick(null);
                picker.DismissViewController(false, null);
                picker.Dispose();
            }
        }
    }
}