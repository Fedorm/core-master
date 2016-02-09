using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Provider;
using BitMobile.ClientModel;
using BitMobile.Common;
using Uri = Android.Net.Uri;

namespace BitMobile.Droid.Providers
{
    class CameraProvider : GalleryProvider, ICameraProvider
    {
        private readonly ApplicationContext _context;
        private string _temporaryFilePath;

        #region ICameraProvider

        public CameraProvider(BaseScreen activity, ApplicationContext context)
            : base(activity)
        {
            _context = context;
        }

        public void MakeSnapshot(string path, int size, Action<object, Camera.CallbackArgs> callback, object state)
        {
            string galleryLastFile = GetLastPhotoInGallery(Activity);
            _temporaryFilePath = Path.Combine(_context.LocalStorage, "temp_from_camera");

            var intent = new Intent(MediaStore.ActionImageCapture);
            intent.PutExtra(MediaStore.ExtraOutput, CreateTempFile());

            Activity.StartActivityForResult(intent
                , BaseScreen.CameraRequestCode
                , (resultCode, data) =>
                {
                    if (resultCode == Result.Ok)
                    {
                        var from = data != null && data.Data != null ? GetPath(data.Data) : _temporaryFilePath;
                        SaveSnapshot(from, path, size, galleryLastFile);
                    }
                    callback(state, new Camera.CallbackArgs(resultCode == Result.Ok));
                });
        }

        #endregion

        private void SaveSnapshot(string from, string to, int size, string galleryLastFile)
        {
            CopyImageFromGallery(from, to, size);

            string inGallery = GetLastPhotoInGallery(Activity);
            if (inGallery != galleryLastFile)
            {
                Activity.ContentResolver.Delete(MediaStore.Images.Media.ExternalContentUri
                    , string.Format("{0}={1}", BaseColumns.Id, inGallery)
                    , null);
            }
        }

        private string GetLastPhotoInGallery(Activity activity)
        {
            string result = null;
            string[] projection = { MediaStore.Images.ImageColumns.Id };
            ICursor cursor = null;
            try
            {
                cursor = activity.ContentResolver
                    .Query(MediaStore.Images.Media.ExternalContentUri
                        , projection
                        , null
                        , null
                        , MediaStore.Images.ImageColumns.DateTaken + " DESC");
                if (cursor.MoveToFirst())
                    result = cursor.GetString(0);
            }
            finally
            {
                if (cursor != null)
                {
                    cursor.Close();
                    cursor.Dispose();
                }
            }

            return result;
        }

        private Uri CreateTempFile()
        {
            string dirPath = Path.GetDirectoryName(_temporaryFilePath);
            if (dirPath != null && !Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            using (var tempFile = new Java.IO.File(_temporaryFilePath))
                return Uri.FromFile(tempFile);
        }
    }
}