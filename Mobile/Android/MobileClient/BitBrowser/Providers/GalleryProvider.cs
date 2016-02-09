using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.Provider;
using BitMobile.ClientModel;
using BitMobile.Common;
using BitMobile.Utilities.Translator;
using Path = System.IO.Path;
using Uri = Android.Net.Uri;

namespace BitMobile.Droid.Providers
{
    class GalleryProvider : IGalleryProvider
    {
        protected readonly BaseScreen Activity;
        
        public GalleryProvider(BaseScreen activity)
        {
            Activity = activity;
        }

        #region IGalleryProvider

        public void Copy(string path, int size, Action<object, Gallery.CallbackArgs> callback, object state)
        {
            var intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);

            Activity.StartActivityForResult(Intent.CreateChooser(intent, D.SELECT_PHOTO)
                , BaseScreen.GalleryRequestCode
                , (resultCode, data) =>
                {
                    if (resultCode == Result.Ok)
                    {
                        var from = GetPath(data.Data);
                        CopyFile(from, path, size, callback, state);
                    }
                    else
                        callback(state, new Gallery.CallbackArgs(false));
                });
        }
        #endregion
        
        protected string GetPath(Uri uri)
        {
            string[] projection = { MediaStore.Images.Media.InterfaceConsts.Data };
            ICursor cursor = Activity.ManagedQuery(uri, projection, null, null, null);
            int index = cursor.GetColumnIndexOrThrow(MediaStore.Images.Media.InterfaceConsts.Data);
            cursor.MoveToFirst();
            return cursor.GetString(index);
        }

        protected bool CopyImageFromGallery(string sourcePath, string destPath, int size)
        {
            try
            {
                var options = new BitmapFactory.Options { InJustDecodeBounds = true };

                BitmapFactory.DecodeFile(sourcePath, options);

                options.InSampleSize = size > 0
                    ? (int)Math.Round((double)Math.Max(options.OutHeight, options.OutWidth) / size)
                    : 1;

                options.InJustDecodeBounds = false;

                Bitmap bitmap = null;
                try
                {
                    bitmap = BitmapFactory.DecodeFile(sourcePath, options);

                    string dirPath = Path.GetDirectoryName(destPath);
                    if (dirPath != null && !Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);

                    using (var stream = new FileStream(destPath, FileMode.Create))
                        bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
                }
                finally
                {
                    if (bitmap != null)
                        bitmap.Dispose();
                }
                return true;
            }
            catch (Exception e)
            {
                BitBrowserApp.Current.ExceptionHandler.HandleNonFatal(e);
                return false;
            }
        }
        
        private void CopyFile(string from, string to, int size, Action<object, Gallery.CallbackArgs> asyncCallback, object state)
        {
            if (File.Exists(to))
            {
                using (var builder = new AlertDialog.Builder(Activity))
                {
                    builder.SetTitle(D.WARNING);
                    builder.SetMessage(D.FILENAME_EXISTS_REPLACE);
                    builder.SetPositiveButton(D.YES,
                        (sender, e) =>
                        {
                            try
                            {
                                File.Delete(to);
                                CopyFile(from, to, size, asyncCallback, state);
                            }
                            catch (IOException)
                            {
                                asyncCallback(state, new Gallery.CallbackArgs(false));
                            }
                        });
                    builder.SetNegativeButton(D.NO, (sender, e) =>
                        asyncCallback(state, new Gallery.CallbackArgs(false)));
                    builder.Show();
                }
            }
            else
            {
                bool result = CopyImageFromGallery(from, to, size);
                asyncCallback(state, new Gallery.CallbackArgs(result));
            }
        }
    }
}