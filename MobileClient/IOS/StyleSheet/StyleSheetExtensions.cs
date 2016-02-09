using System.IO;
using BitMobile.Application;
using BitMobile.Common.StyleSheet;
using BitMobile.UI;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
    public static class StyleSheetExtensions
    {
        public static UIColor ToColorOrClear(this IColorInfo colorInfo)
        {
            return ToNullableColor(colorInfo) ?? UIColor.Clear;
        }

        public static UIColor ToColorOrClear(this IColor color)
        {
            if (color != null)
                return color.Value.ToColorOrClear();
            return UIColor.Clear;
        }

        public static UIColor ToNullableColor(this IColorInfo colorInfo)
        {
            if (colorInfo != null)
                return UIColor.FromRGB(colorInfo.Red, colorInfo.Green, colorInfo.Blue);
            return null;
        }

        public static UIColor ToNullableColor(this IColor color)
        {
            if (color != null)
                return color.Value.ToNullableColor();
            return null;
        }

        public static UIImage SetBackgroundSettings(this IStyleSheet styleSheet, Control control)
        {
            IStyleSheetHelper style = styleSheet.Helper;

            UIImage image = styleSheet.GetBackgroundImage(control);
            if (image != null)
                return image;

            control.View.BackgroundColor = style.BackgroundColor(control).ToColorOrClear();
            if (style.HasBorder(control))
            {
                control.View.ClipsToBounds = true;
                control.View.Layer.BorderWidth = style.BorderWidth(control);
                control.View.Layer.BorderColor = style.BorderColor(control).ToColorOrClear().CGColor;
                control.View.Layer.CornerRadius = style.BorderRadius(control);
            }
            return null;
        }

        public static UIImage SetBackgroundSettings(this IStyleHelper helper, Control control)
        {
            IBackgroundImage backgroundImage;
            if (helper.TryGet(out backgroundImage) && !string.IsNullOrWhiteSpace(backgroundImage.Path))
            {
                UIImage image = backgroundImage.GetImage();
                if (image != null)
                    return image;
            }
            // if control has background image, we have to ignore background color
            if (!string.IsNullOrEmpty(backgroundImage.Path))
                return null;

            IBackgroundColor backgroundColor;
            IBorderStyle borderStyle;
            IBorderWidth borderWidth;
            IBorderColor borderColor;
            IBorderRadius borderRadius;
            if (helper.TryGet(out backgroundColor) | helper.TryGet(out borderStyle) | helper.TryGet(out borderWidth)
                | helper.TryGet(out borderColor) | helper.TryGet(out borderRadius))
            {
                control.View.BackgroundColor = backgroundColor.ToColorOrClear();
                if (borderStyle.Style == BorderStyleValues.Solid)
                {
                    control.View.ClipsToBounds = true;
                    control.View.Layer.BorderWidth = borderWidth.Value;
                    control.View.Layer.BorderColor = borderColor.ToColorOrClear().CGColor;
                    control.View.Layer.CornerRadius = borderRadius.Radius;
                }
                else
                {
                    // hide bounds
                    control.View.ClipsToBounds = false;
                    control.View.Layer.BorderWidth = 0;
                    control.View.Layer.BorderColor = UIColor.Clear.CGColor;
                    control.View.Layer.CornerRadius = 0;
                }
            }
            return null;
        }

        public static UIImage GetBackgroundImage(this IStyleSheet styleSheet, Control control)
        {
            string imgPath = styleSheet.Helper.BackgroundImage(control);
            return GetImage(imgPath);
        }

        public static UIImage GetImage(this IBackgroundImage style)
        {
            return GetImage(style.Path);
        }

        public static UIFont Font(this IStyleSheet styleSheet, Control control, float parentHeight)
        {
            IStyleSheetHelper style = styleSheet.Helper;

            string fontName = style.FontName(control);
            if (fontName != null)
            {
                float size = style.FontSize(control, parentHeight);
                return UIFont.FromName(fontName, size);
            }
            return null;
        }

        public static bool FontChanged(this IStyleHelper helper, float parentHeight, out UIFont font)
        {
            string fontFamily;
            float fontSize;
            bool changed = helper.TryGetFontFamily(out fontFamily);
            changed |= helper.TryGetFontSize(parentHeight, out fontSize);

            if (changed)
            {
                font = UIFont.FromName(fontFamily, fontSize);
                return true;
            }

            font = null;
            return false;
        }

        private static UIImage GetImage(string imgPath)
        {
            if (imgPath != null)
            {
                Stream imgStream = ApplicationContext.Current.Dal.GetImageByName(imgPath);
                if (imgStream != null)
                    return UIImage.LoadFromData(NSData.FromStream(imgStream));
            }
            return null;
        }
    }
}