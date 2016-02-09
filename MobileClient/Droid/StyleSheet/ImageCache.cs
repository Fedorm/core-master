using System;
using System.Collections.Generic;
using System.IO;
using Android.Graphics;
using Android.Graphics.Drawables;
using BitMobile.Common.Application;

namespace BitMobile.Droid.StyleSheet
{
    // todo: убрать логику, опирающуюся на диагональ. Использовать для этого inSampleSize изображения
    class ImageCache : IDisposable
    {
        private IApplicationContext _applicationContext;
        private Dictionary<string, Dictionary<double, Bitmap>> _cache = new Dictionary<string, Dictionary<double, Bitmap>>();
        bool _disposed;

        public ImageCache(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public BitmapDrawable GetImage(string imgPath, float width, float height)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            Bitmap img = null;
            Dictionary<double, Bitmap> images;
            if (_cache.TryGetValue(imgPath, out images))
            {
                foreach (var item in images)
                {
                    double diagonal = GetDiagonal(item.Value, width, height);
                    if (Math.Abs(item.Key - diagonal) < 0.01)
                        img = item.Value;
                }

                if (img == null)
                {
                    double diagonal;
                    img = LoadBitmap(imgPath, width, height, out diagonal);
                    images.Add(diagonal, img);
                }
            }
            else
            {
                double diagonal;
                img = LoadBitmap(imgPath, width, height, out diagonal);
                images = new Dictionary<double, Bitmap> { { diagonal, img } };
                _cache.Add(imgPath, images);
            }

            return new BitmapDrawable(img);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _applicationContext = null;

                if (_cache != null)
                    _cache.Clear();
                _cache = null;

                _disposed = true;
            }
        }

        private Bitmap LoadBitmap(string imgPath, float width, float height, out double diagonal)
        {
            using (Stream stream = _applicationContext.Dal.GetImageByName(imgPath))
            {
                var result = Helper.LoadBitmap(stream, width.Round(), height.Round());
                diagonal = GetDiagonal(result, width, height);
                return result;
            }
        }

        private double GetDiagonal(Bitmap image, float width, float height)
        {
            float proportion = (float)image.Width / image.Height;
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (width == 0 && height != 0)
                width = (int)Math.Round(height * proportion);
            else if (height == 0 && width != 0)
                height = (int)Math.Round(width / proportion);
            // ReSharper restore CompareOfFloatsByEqualityOperator
            return Math.Sqrt(Math.Pow(width, 2) + Math.Pow(height, 2));
        }

    }
}
