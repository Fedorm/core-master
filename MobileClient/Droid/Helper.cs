using System;
using System.IO;
using Android.Content.Res;
using Android.Graphics;
using Android.Widget;
using BitMobile.Application.Translator;
using Java.Lang;
using Math = System.Math;

namespace BitMobile.Droid
{
    public static class Helper
    {
        public static int Round(this float obj)
        {
            return (int)Math.Round(obj);
        }

        public static Bitmap LoadBitmap(int resId, float width, float height, Resources res)
        {
            return LoadBitmapTemplate(resId, width, height, true
                , (id, options) => BitmapFactory.DecodeResource(res, id, options));
        }

        public static Bitmap LoadBitmap(string path, float width, float height, bool maxResoltion)
        {
            return LoadBitmapTemplate(path, width, height, maxResoltion, BitmapFactory.DecodeFile);
        }

        public static Bitmap LoadBitmap(Stream stream, float width, float height)
        {
            return LoadBitmapTemplate(stream, width, height, true
                , (source, options) =>
                {
                    stream.Flush();
                    stream.Position = 0;
                    return BitmapFactory.DecodeStream(source, null, options);
                });
        }

        public static Bitmap LoadBitmap(Func<Stream> factory, float width, float height)
        {
            using (Stream preparingStream = factory())
                return LoadBitmapTemplate(preparingStream, width, height, false
                    , (source, options) =>
                    {
                        using (Stream stream = factory())
                            return BitmapFactory.DecodeStream(stream, null, options);
                    });
        }

        private static Bitmap LoadBitmapTemplate<T>(T source, float width, float height, bool maxResolution
            , Func<T, BitmapFactory.Options, Bitmap> decode)
        {
            using (var options = new BitmapFactory.Options())
            {
                if (width > 0 || height > 0)
                {
                    options.InJustDecodeBounds = true;

                    decode(source, options);

                    options.InSampleSize = CalculateInSampleSize(options, width, height, maxResolution);

                    options.InJustDecodeBounds = false;
                }
                else
                    options.InSampleSize = 1;

                do
                {
                    bool exceptionThrowed = false;
                    try
                    {
                        return decode(source, options);
                    }
                    catch (OutOfMemoryError)
                    {
                        exceptionThrowed = true;
                        options.InSampleSize *= 2;
                    }
                    finally
                    {
                        if (BitBrowserApp.Current.AppContext.Settings.DevelopModeEnabled && exceptionThrowed)
                            Toast.MakeText(BitBrowserApp.Current.BaseActivity, D.IMAGE_WAS_RESIZED, ToastLength.Long).Show();
                    }
                } while (true);
            }
        }

        static int CalculateInSampleSize(BitmapFactory.Options options, float reqWidth, float reqHeight, bool maxResolution)
        {
            float height = options.OutHeight;
            float width = options.OutWidth;
            int inSampleSize = 1;

            if (height > reqHeight || width > reqWidth)
            {
                // Calculate the largest inSampleSize value that is a power of 2 and keeps both
                // height and width larger than the requested height and width.
                while ((height / inSampleSize) > reqHeight && (width / inSampleSize) > reqWidth)
                    inSampleSize *= 2;

                if (maxResolution && inSampleSize > 1)
                    inSampleSize /= 2;
            }

            return inSampleSize;
        }
    }
}