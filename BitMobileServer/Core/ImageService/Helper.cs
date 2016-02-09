using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace ImageService
{
    class Helper
    {
        private static Dictionary<String, Image> images = new Dictionary<string, Image>();

        public static System.IO.MemoryStream ImageFromText(String text)
        {
            Image image = null;
            if (images.ContainsKey(text))
                image = images[text];
            else
            {
                Font font = new Font("Arial", 12);
                image = new Bitmap(1, 1);
                Graphics drawing = Graphics.FromImage(image);

                //measure the string to see how big the image needs to be
                SizeF textSize = drawing.MeasureString(text, font);

                //free up the dummy image and old graphics object
                image.Dispose();
                drawing.Dispose();

                //create a new image of the right size
                image = new Bitmap((int)textSize.Width, (int)textSize.Height);

                drawing = Graphics.FromImage(image);

                //paint the background
                drawing.Clear(Color.White);

                //create a brush for the text
                Brush textBrush = new SolidBrush(Color.Black);

                drawing.DrawString(text, font, textBrush, 0, 0);

                drawing.Save();

                textBrush.Dispose();
                drawing.Dispose();

                images.Add(text, image);
            }

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg); 
            ms.Position = 0;
            return ms;
        }

    }
}
