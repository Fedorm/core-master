using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using BitMobile.Application;
using BitMobile.Controls;
using BitMobile.Controls.StyleSheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TextView = Android.Widget.TextView;

namespace BitMobile.Droid
{
    sealed class StyleHelper : IStyleSheetHelper, IDisposable
    {
        AndroidStyleSheet _stylesheet;

        Dictionary<string, BitmapDrawable> _imagesCache = new Dictionary<string, BitmapDrawable>();

        bool _disposed = false;

        public StyleHelper(AndroidStyleSheet stylesheet)
        {
            _stylesheet = stylesheet;
        }

        #region Size

        float Size<T>(IStyledObject control, float parentSize, int displayMetric) where T : Size
        {
            float result = 0;

            Dictionary<Type, Style> styles = _stylesheet.GetStyles(control);

            if (styles.ContainsKey(typeof(T)))
            {
                Size size = (Size)styles[typeof(T)];

                switch (size.Measure)
                {
                    case Measure.Pixels:
                        result = size.Value;
                        break;
                    case Measure.Percent:
                        result = size.Value * parentSize / 100;
                        break;
                    case Measure.ScreenPercent:
                        result = size.Value * displayMetric / 100;
                        break;
                    case Measure.Millimetre:
                        {
                            var metrics = BitBrowserApp.Current.Resources.DisplayMetrics;
                            double px = size.Value * (int)metrics.DensityDpi / 25.4;
                            result = (int)Math.Round(px);
                        }
                        break;
                    case Measure.None:
                        result = size.Value;
                        break;
                    default:
                        result = size.Value;
                        break;
                }
            }

            return result;
        }

        private bool SizeToContent<T>(IStyledObject control)
        {
            bool result = false;

            Dictionary<Type, Style> styles = _stylesheet.GetStyles(control);
            if (styles.ContainsKey(typeof(T)))
            {
                var size = (Size)styles[typeof(T)];
                result = size.SizeToContent;
            }

            return result;
        }

        public float Width(IStyledObject control, float parentSize)
        {
            float r = Size<Width>(control, parentSize, BitBrowserApp.Current.Width);
            return r;
        }

        public float Height(IStyledObject control, float parentSize)
        {
            float r = Size<Height>(control, parentSize, BitBrowserApp.Current.Height);
            return r;
        }

        public float Margin<T>(IStyledObject control, float parentSize) where T : Margin
        {
            int displayMetric;
            if (typeof(T) == typeof(MarginLeft) || typeof(T) == typeof(MarginRight))
                displayMetric = BitBrowserApp.Current.Width;
            else if (typeof(T) == typeof(MarginTop) || typeof(T) == typeof(MarginBottom))
                displayMetric = BitBrowserApp.Current.Height;
            else
                throw new ArgumentException("Type is not Margin type " + typeof(T).ToString());

            float r = Size<T>(control, parentSize, displayMetric);
            return r;
        }

        public float Padding<T>(IStyledObject control, float parentSize) where T : Padding
        {
            int displayMetric;
            if (typeof(T) == typeof(PaddingLeft) || typeof(T) == typeof(PaddingRight))
                displayMetric = BitBrowserApp.Current.Width;
            else if (typeof(T) == typeof(PaddingTop) || typeof(T) == typeof(PaddingBottom))
                displayMetric = BitBrowserApp.Current.Height;
            else
                throw new ArgumentException("Type is not Padding type " + typeof(T).ToString());

            float r = Size<T>(control, parentSize, displayMetric);
            return r;
        }

        public bool HasBorder(IStyledObject control)
        {
            Dictionary<Type, Style> styles = _stylesheet.GetStyles(control);

            if (styles.ContainsKey(typeof(Border)))
            {
                Border border = (Border)styles[typeof(Border)];
                if (border.Value == "solid")
                    return true;
                return false;
            }
            else
                return false;
        }

        public float BorderWidth(IStyledObject control)
        {
            float r = Size<BorderWidth>(control, 0, 0);
            return r;
        }

        public float BorderRadius(IStyledObject control)
        {
            float r = Size<BorderRadius>(control, 0, 0);
            return r;
        }

        public DockAlign.Align DockAlign(IStyledObject control)
        {
            Dictionary<Type, Style> styles = _stylesheet.GetStyles(control);

            if (styles.ContainsKey(typeof(DockAlign)))
            {
                DockAlign align = (DockAlign)styles[typeof(DockAlign)];
                return align.Value;
            }
            else
                return Controls.StyleSheet.DockAlign.Align.Top;
        }

        public HorizontalAlign.Align HorizontalAlign(IStyledObject control)
        {
            Dictionary<Type, Style> styles = _stylesheet.GetStyles(control);

            if (styles.ContainsKey(typeof(HorizontalAlign)))
            {
                HorizontalAlign align = (HorizontalAlign)styles[typeof(HorizontalAlign)];
                return align.Value;
            }
            else
                return Controls.StyleSheet.HorizontalAlign.Align.Left;
        }

        public VerticalAlign.Align VerticalAlign(IStyledObject control)
        {
            Dictionary<Type, Style> styles = _stylesheet.GetStyles(control);

            if (styles.ContainsKey(typeof(VerticalAlign)))
            {
                VerticalAlign align = (VerticalAlign)styles[typeof(VerticalAlign)];
                return align.Value;
            }
            else
                return Controls.StyleSheet.VerticalAlign.Align.Top;
        }

        public bool SizeToContentWidth(IStyledObject control)
        {
            return SizeToContent<Width>(control);
        }
        
        public bool SizeToContentHeight(IStyledObject control)
        {
            return SizeToContent<Height>(control);
        }

        #endregion

        #region Drawable

        public Android.Graphics.Color? Color<T>(IStyledObject control) where T : BitMobile.Controls.StyleSheet.Color
        {
            Android.Graphics.Color? result;

            Dictionary<Type, Style> styles = _stylesheet.GetStyles(control);

            if (styles.ContainsKey(typeof(T)))
            {
                var color = (BitMobile.Controls.StyleSheet.Color)styles[typeof(T)];

                result = FromHexString(color.Value);
            }
            else
                result = null;

            return result;
        }

        public Android.Graphics.Color ColorOrTransparent<T>(IStyledObject control) where T : BitMobile.Controls.StyleSheet.Color
        {
            var result = Color<T>(control);
            return result ?? Android.Graphics.Color.Transparent;
        }

        public bool InitializeImageContainer(IImageContainer container, IApplicationContext context)
        {
            String imgPath = BackgroundImage(container);
            if (imgPath != null)
            {
                BitmapDrawable img = GetImage(context, imgPath);
                container.ImageWidth = img.Bitmap.Width;
                container.ImageHeight = img.Bitmap.Height;
                return true;
            }
            return false;
        }

        public Drawable Background(IControl<View> control, IApplicationContext context, bool whithoutImage = false)
        {
            Drawable result = null;

            if (!whithoutImage)
            {
                string path = BackgroundImage(control);
                if (path != null)
                    result = GetImage(context, path);
            }

            if (result == null)
                result = ColorWithBorders(control, ColorOrTransparent<BackgroundColor>(control));

            return result;
        }

        public Drawable ColorWithBorders(IControl<View> control, Android.Graphics.Color color)
        {
            Drawable drawable = null;
            if (HasBorder(control))
            {
                GradientDrawable shape = new GradientDrawable();
                int borderWidth = (int)Math.Round(BorderWidth(control));
                var borderColor = ColorOrTransparent<BorderColor>(control);

                shape.SetShape(ShapeType.Rectangle);
                shape.SetColor(color);
                shape.SetCornerRadius(BorderRadius(control));
                shape.SetStroke(borderWidth, borderColor);
                drawable = shape;
            }
            else
                drawable = new ColorDrawable(color);
            return drawable;
        }

        BitmapDrawable GetImage(IApplicationContext applicationContext, String imgPath)
        {
            BitmapDrawable img;
            if (_imagesCache.ContainsKey(imgPath))
            {
                img = _imagesCache[imgPath];
            }
            else
                using (Stream stream = applicationContext.DAL.GetImageByName(imgPath))
                {
                    img = (BitmapDrawable)Drawable.CreateFromStream(stream, imgPath);
                    _imagesCache.Add(imgPath, img);
                }
            return img;
        }

        static Android.Graphics.Color? FromHexString(string hexValue, int alpha = 255)
        {
            if (hexValue.ToLower().Equals("none"))
                return null;

            var colorString = hexValue.Replace("#", "");
            if (alpha > 255)
                alpha = 255;
            else if (alpha < 0)
                alpha = 0;

            int red, green, blue;

            switch (colorString.Length)
            {
                case 3: // #RGB
                    {
                        red = Convert.ToInt32(string.Format("{0}{0}", colorString.Substring(0, 1)), 16);
                        green = Convert.ToInt32(string.Format("{0}{0}", colorString.Substring(1, 1)), 16);
                        blue = Convert.ToInt32(string.Format("{0}{0}", colorString.Substring(2, 1)), 16);
                        return new global::Android.Graphics.Color(red, green, blue, alpha);
                    }
                case 6: // #RRGGBB
                    {
                        red = Convert.ToInt32(colorString.Substring(0, 2), 16);
                        green = Convert.ToInt32(colorString.Substring(2, 2), 16);
                        blue = Convert.ToInt32(colorString.Substring(4, 2), 16);
                        return new global::Android.Graphics.Color(red, green, blue, alpha);
                    }

                default:
                    throw new ArgumentOutOfRangeException(string.Format("Invalid color value {0} is invalid. It should be a hex value of the form #RBG, #RRGGBB", hexValue));

            }
        }

        #endregion

        #region Font

        public float FontSize(IControl<View> control, float height)
        {
            float result = 0;

            Dictionary<Type, Style> styles = _stylesheet.GetStyles(control);

            if (styles.ContainsKey(typeof(Font)))
            {
                Font font = (Font)styles[typeof(Font)];

                switch (font.Measure)
                {
                    case Measure.Pixels:
                        result = font.Size;
                        break;
                    case Measure.Percent:
                        result = font.Size * height / 100;
                        break;
                    case Measure.ScreenPercent:
                        result = font.Size * BitBrowserApp.Current.Height / 100;
                        break;
                    case Measure.Millimetre:
                        {
                            float px = TypedValue.ApplyDimension(ComplexUnitType.Mm
                                , font.Size
                                , BitBrowserApp.Current.Resources.DisplayMetrics);
                            result = (int)Math.Round(px);
                        }
                        break;
                    case Measure.None:
                        result = font.Size;
                        break;
                    default:
                        result = font.Size;
                        break;
                }
            }

            return (int)Math.Round(result);
        }

        public TextAlign.Align TextAlign(IStyledObject control)
        {
            Dictionary<Type, Style> styles = _stylesheet.GetStyles(control);

            if (styles.ContainsKey(typeof(TextAlign)))
            {
                TextAlign align = (TextAlign)styles[typeof(TextAlign)];
                return align.Value;
            }
            else
                return Controls.StyleSheet.TextAlign.Align.Left;
        }

        public TextFormat.Format TextFormat(IStyledObject control)
        {
            Dictionary<Type, Style> styles = _stylesheet.GetStyles(control);

            if (styles.ContainsKey(typeof(TextFormat)))
            {
                TextFormat format = (TextFormat)styles[typeof(TextFormat)];
                return format.Value;
            }
            else
                return BitMobile.Controls.StyleSheet.TextFormat.Format.Text;
        }

        public void SetFontSettings(IControl<View> control, TextView view, float height)
        {
            using (Typeface font = Font(control))
            {
                if (font != null)
                {
                    view.SetTypeface(font, TypefaceStyle.Normal);
                    view.SetTextSize(ComplexUnitType.Px, FontSize(control, height));
                }
            }
        }

        public WhiteSpace.Kind WhiteSpaceKind(IStyledObject control)
        {
            Dictionary<Type, Style> styles = _stylesheet.GetStyles(control);

            if (styles.ContainsKey(typeof(WhiteSpace)))
            {
                WhiteSpace ws = (WhiteSpace)styles[typeof(WhiteSpace)];
                return ws.Value;
            }
            else
                return WhiteSpace.Kind.Nowrap;
        }

        Typeface Font(IControl<View> control)
        {
            Typeface result = null;

            Dictionary<Type, Style> styles = _stylesheet.GetStyles(control);

            if (styles.ContainsKey(typeof(Font)))
            {
                Font font = (Font)styles[typeof(Font)];

                result = Typeface.Create(font.Name, TypefaceStyle.Normal);
            }

            return result;
        }

        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~StyleHelper()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _stylesheet = null;
                }

                foreach (BitmapDrawable img in _imagesCache.Values)
                    img.Dispose();
                _imagesCache.Clear();
                _imagesCache = null;

                _disposed = true;
            }
        }

        String BackgroundImage(object view)
        {
            Dictionary<Type, Style> styles = _stylesheet.GetStyles(view);

            if (styles.ContainsKey(typeof(BackgroundImage)))
            {
                BackgroundImage img = (BackgroundImage)styles[typeof(BackgroundImage)];
                return img.Path;
            }
            else
                return null;
        }

    }
}