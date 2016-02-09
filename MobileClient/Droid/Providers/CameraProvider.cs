using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.Provider;
using BitMobile.Common.Device.Providers;
using BitMobile.Droid.Application;

namespace BitMobile.Droid.Providers
{
    class CameraProvider : GalleryProvider, ICameraProvider
    {
        public CameraProvider(BaseScreen activity, AndroidApplicationContext context)
            : base(activity, context)
        {
        }

        #region ICameraProvider

        public async void MakeSnapshot(string path, int size, Action<bool> callback)
        {
            var intent = new Intent(MediaStore.ActionImageCapture);

            string galleryLastFile = GetLastPhotoInGallery(Activity);

            bool success;
            using (Intent data = await Present(intent))
                success = await SaveSnapshot(data, path, size, galleryLastFile);

            callback(success);
        }

        #endregion

        private async Task<bool> SaveSnapshot(Intent data, string to, int size, string galleryLastFile)
        {
            bool result = await CopyFile(data, to, size);

            if (result)
            {
                string inGallery = GetLastPhotoInGallery(Activity);
                if (inGallery != galleryLastFile)
                {
                    Activity.ContentResolver.Delete(MediaStore.Images.Media.ExternalContentUri
                        , string.Format("{0}={1}", BaseColumns.Id, inGallery)
                        , null);
                }
            }
            return result;
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
    }
}