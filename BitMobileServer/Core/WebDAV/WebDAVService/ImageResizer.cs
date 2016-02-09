using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMWebDAV
{
    public class ImageResizer
    {
        const int size = 100;


        public static Stream TryResize(String path, Stream input, NameValueCollection parameters)
        {
            if (GetContentType(path) != "")
            {
                foreach (String key in parameters)
                {
                    if (key.ToLower().Equals("size"))
                    {
                        int size;
                        if (int.TryParse(parameters[key], out size))
                        {
                            return GetResizedImageInternal(path, input, size, size);
                        }
                    }
            }
            }
            return input;
        }

        public static string GetContentType(String path)
        {
            switch (Path.GetExtension(path))
            {
                case ".bmp": return "Image/bmp";
                case ".gif": return "Image/gif";
                case ".jpg": return "Image/jpeg";
                case ".png": return "Image/png";
                default: break;
            }
            return "";
        }

        static Stream GetResizedImageInternal(String path, Stream input, int width, int height)
        {
            Bitmap imgIn = new Bitmap(input);
            double y = imgIn.Height;
            double x = imgIn.Width;

            double factor = 1;
            if (width > 0)
            {
                factor = width / x;
            }
            else if (height > 0)
            {
                factor = height / y;
            }

            System.IO.MemoryStream outStream = new System.IO.MemoryStream();
            using (Bitmap imgOut = new Bitmap((int)(x * factor), (int)(y * factor)))
            {
                // Set DPI of image (xDpi, yDpi)
                imgOut.SetResolution(72, 72);

                using (Graphics g = Graphics.FromImage(imgOut))
                {
                    g.Clear(Color.White);
                    g.DrawImage(imgIn, new Rectangle(0, 0, (int)(factor * x), (int)(factor * y)),
                        new Rectangle(0, 0, (int)x, (int)y), GraphicsUnit.Pixel);

                    imgOut.Save(outStream, GetImageFormat(path));
                    outStream.Position = 0;

                    return outStream;
                }
            }
        }

        static ImageFormat GetImageFormat(String path)
        {
            switch (Path.GetExtension(path))
            {
                case ".bmp": return ImageFormat.Bmp;
                case ".gif": return ImageFormat.Gif;
                case ".jpg": return ImageFormat.Jpeg;
                case ".png": return ImageFormat.Png;
                default: break;
            }
            return ImageFormat.Jpeg;
        }
    }
}
