using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using BitMobile.Application.IO;
using BitMobile.Application.Translator;
using BitMobile.Common.Device.Providers;
using BitMobile.Common.IO;
using BitMobile.Droid.Application;
using Java.Lang;
using Exception = System.Exception;
using Path = System.IO.Path;
using Uri = Android.Net.Uri;

namespace BitMobile.Droid.Providers
{
    class GalleryProvider : IGalleryProvider
    {
        protected readonly BaseScreen Activity;
        private readonly AndroidApplicationContext _context;
        private string _temporaryFilePath;

        public GalleryProvider(BaseScreen activity, AndroidApplicationContext context)
        {
            Activity = activity;
            _context = context;
        }

        #region IGalleryProvider

        public async void Copy(string path, int size, Action<bool> callback)
        {
            var intent = new Intent();
            intent.SetType("image/*");

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                intent.SetAction(Intent.ActionOpenDocument);
                intent.PutExtra(Intent.ExtraAllowMultiple, false);
                intent.AddCategory(Intent.CategoryOpenable);
                intent.AddFlags(ActivityFlags.GrantPersistableUriPermission);
            }
            else
                intent.SetAction(Intent.ActionGetContent);

            bool success;
            using (Intent data = await Present(intent))
                success = await CopyFile(data, path, size);

            callback(success);
        }
        #endregion

        protected async Task<Intent> Present(Intent intent)
        {
            _temporaryFilePath = Path.Combine(_context.LocalStorage, "temp_from_camera");

            intent.PutExtra(MediaStore.ExtraOutput, CreateTempFile());

            BaseScreen.ActivityResult result = await Activity.StartActivityForResultAsync(intent);

            if (result.Result == Result.Ok)
                return result.Data;
            return null;
        }

        protected Task<bool> CopyFile(Intent data, string to, int size)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (data == null && !File.Exists(_temporaryFilePath))
            {
                tcs.SetResult(false);
                return tcs.Task;
            }

            if (IOContext.Current.Exists(to))
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
                                IOContext.Current.Delete(to);
                                Task<bool> task = CopyFile(data, to, size);
                                task.ContinueWith(t => tcs.SetResult(t.Result));
                            }
                            catch (IOException)
                            {
                                tcs.SetResult(false);
                            }
                        });
                    builder.SetNegativeButton(D.NO, (sender, e) => tcs.SetResult(false));
                    builder.Show();
                }
            }
            else
            {
                bool result = CopyImageFromGallery(data, to, size);
                tcs.SetResult(result);
            }
            return tcs.Task;
        }


        private bool CopyImageFromGallery(Intent data, string destPath, int size)
        {
            Bitmap bitmap = null;
            try
            {
                string sourcePath;
                if (TryGetPath(data, out sourcePath))
                {
                    if (size == 0)
                    {
                        string dir = Path.GetDirectoryName(destPath);
                        if (dir != null && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        File.Copy(sourcePath, destPath);
                        return true;
                    }
                    bitmap = Helper.LoadBitmap(sourcePath, size, size, false);
                }
                else if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
                {
                    Uri uri = data.Data;
                    Activity.ContentResolver.TakePersistableUriPermission(uri
                        , data.Flags & (ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission));
                    //var stream = Activity.ContentResolver.OpenInputStream(uri);
                    bitmap = Helper.LoadBitmap(() => Activity.ContentResolver.OpenInputStream(uri), size, size);
                }
                else
                    throw new NotImplementedException("Unexpected behavior");


                IIOContext io = IOContext.Current;
                string dirPath = Path.GetDirectoryName(destPath);
                if (dirPath != null)
                    io.CreateDirectory(dirPath);

                using (var stream = io.FileStream(destPath, FileMode.Create))
                    bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);


                return true;
            }
            catch (Exception e)
            {
                BitBrowserApp.Current.ExceptionHandler.HandleNonFatal(e);
                return false;
            }
            finally
            {
                if (bitmap != null)
                    bitmap.Dispose();
            }
        }

        private bool TryGetPath(Intent data, out string path)
        {
            if (data != null && data.Data != null)
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
                {
                    Uri uri = data.Data;
                    string[] projection = { MediaStore.Images.Media.InterfaceConsts.Data };
                    ICursor cursor = Activity.ContentResolver.Query(uri, projection, null, null, null);
                    int index = cursor.GetColumnIndexOrThrow(MediaStore.Images.Media.InterfaceConsts.Data);
                    cursor.MoveToFirst();
                    path = cursor.GetString(index);
                    return true;
                }
                path = null;
                return false;
            }
            path = _temporaryFilePath;
            return true;
        }

        private Uri CreateTempFile()
        {
            string path = _temporaryFilePath;
            if (File.Exists(path))
                File.Delete(path);

            string dirPath = Path.GetDirectoryName(path);
            if (dirPath != null && !Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            using (var tempFile = new Java.IO.File(path))
                return Uri.FromFile(tempFile);
        }
    }
}