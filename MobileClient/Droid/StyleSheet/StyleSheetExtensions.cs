using System;
using System.Diagnostics;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.Droid.UI;

namespace BitMobile.Droid.StyleSheet
{
    internal static class StyleSheetExtensions
    {
        public static Color ToColorOrTransparent(this IColorInfo colorInfo)
        {
            return ToNullableColor(colorInfo) ?? Color.Transparent;
        }

        public static Color? ToNullableColor(this IColorInfo colorInfo)
        {
            if (colorInfo != null)
                return new Color(colorInfo.Red, colorInfo.Green, colorInfo.Blue);
            return null;
        }

        public static Color ToColorOrTransparent(this IColor style)
        {
            return ToColorOrTransparent(style.Value);
        }

        public static Color? ToNullableColor(this IColor style)
        {
            if (style != null)
                return ToNullableColor(style.Value);
            return null;
        }

        public static Drawable Background(this IStyleSheet styleSheet, IStyledObject control, IBound bound, bool whithoutImage = false)
        {
            Drawable background = null;
            if (!whithoutImage)
            {
                string path = styleSheet.Helper.BackgroundImage(control);
                if (path != null)
                    background = styleSheet.GetCache<ImageCache>().GetImage(path, bound.Width, bound.Height);
            }

            return background ??
                ColorWithBorders(styleSheet, control, ToColorOrTransparent(styleSheet.Helper.BackgroundColor(control)));
        }

        public static void ReloadBackgroundImage(this IStyleHelper helper, IStyleSheet styleSheet, IBound bound, Control control)
        {
            IBackgroundImage image;
            if (helper.TryGet(out image) && !string.IsNullOrWhiteSpace(image.Path))
            {
                var imageCache = styleSheet.GetCache<ImageCache>();
                using (BitmapDrawable background = imageCache.GetImage(image.Path, bound.Width, bound.Height))
                    control.SetBackground(background);
            }
        }

        public static bool BackgroundChanged(this IStyleHelper helper
            , IStyleSheet styleSheet, IBound bound, out Drawable background, bool whithoutImage = false)
        {
            if (!whithoutImage)
            {
                IBackgroundImage image;
                if (helper.TryGet(out image) && !string.IsNullOrWhiteSpace(image.Path))
                {
                    background = styleSheet.GetCache<ImageCache>().GetImage(image.Path, bound.Width, bound.Height);
                    return true;
                }

                // if control has background image, we have to ignore background color
                if (!string.IsNullOrWhiteSpace(image.Path))
                {
                    background = null;
                    return false;
                }
            }

            IBackgroundColor backgroundColor;
            IBorderStyle borderStyle;
            IBorderWidth borderWidth;
            IBorderColor borderColor;
            IBorderRadius borderRadius;
            if (helper.TryGet(out backgroundColor) | helper.TryGet(out borderStyle) | helper.TryGet(out borderWidth)
                | helper.TryGet(out borderColor) | helper.TryGet(out borderRadius))
            {
                if (borderStyle.Style == BorderStyleValues.Solid)
                {
                    var shape = new GradientDrawable();
                    var width = (int)Math.Round(borderWidth.Value);
                    Color color = borderColor.ToColorOrTransparent();

                    shape.SetShape(ShapeType.Rectangle);
                    shape.SetColor(backgroundColor.ToColorOrTransparent());
                    shape.SetCornerRadius(borderRadius.Radius);
                    shape.SetStroke(width, color);
                    background = shape;
                }
                else
                    background = new ColorDrawable(backgroundColor.ToColorOrTransparent());
                return true;
            }

            background = null;
            return false;
        }

        public static void SetFontSettings(this IStyleHelper helper, TextView view, float parentHeight)
        {
            string fontFamily;
            if (helper.TryGetFontFamily(out fontFamily))
                view.SetTypeface(Typeface.Create(fontFamily, TypefaceStyle.Normal), TypefaceStyle.Normal);

            float fontSize;
            if (helper.TryGetFontSize(parentHeight, out fontSize))
                view.SetTextSize(ComplexUnitType.Px, fontSize);
        }

        public static Drawable ColorWithBorders(this IStyleSheet styleSheet, IStyledObject control, Color color)
        {
            IStyleSheetHelper helper = styleSheet.Helper;
            Drawable drawable;
            if (styleSheet.Helper.HasBorder(control))
            {
                var shape = new GradientDrawable();
                var borderWidth = (int)Math.Round(helper.BorderWidth(control));
                Color borderColor = ToColorOrTransparent(helper.BorderColor(control));

                shape.SetShape(ShapeType.Rectangle);
                shape.SetColor(color);
                shape.SetCornerRadius(helper.BorderRadius(control));
                shape.SetStroke(borderWidth, borderColor);
                drawable = shape;
            }
            else
                drawable = new ColorDrawable(color);
            return drawable;
        }

        public static void SetFontSettings(this IStyleSheet styleSheet, IControl<View> control, TextView view, float parentHeight)
        {
            using (Typeface font = Font(styleSheet, control))
                if (font != null)
                {
                    view.SetTypeface(font, TypefaceStyle.Normal);
                    view.SetTextSize(ComplexUnitType.Px, styleSheet.Helper.FontSize(control, parentHeight));
                }
        }

        static Typeface Font(this IStyleSheet styleSheet, IStyledObject control)
        {
            string name = styleSheet.Helper.FontName(control);
            if (name != null)
                return Typeface.Create(name, TypefaceStyle.Normal);
            return null;
        }
    }
}