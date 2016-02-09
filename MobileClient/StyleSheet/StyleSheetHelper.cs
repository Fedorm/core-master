using System;
using System.Collections.Generic;
using BitMobile.Application;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    [Obsolete]
    class StyleSheetHelper : IStyleSheetHelper
    {
        private readonly Func<IStyledObject, IDictionary<Type, IStyle>> _getStyles;
        //private readonly IStyleSheet _stylesheet;

        public StyleSheetHelper(Func<IStyledObject, IDictionary<Type, IStyle>> getStyles)
        {
            _getStyles = getStyles;
        }

        #region Size

        public float Width(IStyledObject control, float parentSize)
        {
            return Size<Width>(control, parentSize, ApplicationContext.Current.DisplayProvider.Width);
        }

        public float Height(IStyledObject control, float parentSize)
        {
            return Size<Height>(control, parentSize, ApplicationContext.Current.DisplayProvider.Height);
        }

        public float MarginLeft(IStyledObject control, float parentSize)
        {
            return Margin<MarginLeft>(control, parentSize);
        }

        public float MarginTop(IStyledObject control, float parentSize)
        {
            return Margin<MarginTop>(control, parentSize);
        }

        public float MarginRight(IStyledObject control, float parentSize)
        {
            return Margin<MarginRight>(control, parentSize);
        }

        public float MarginBottom(IStyledObject control, float parentSize)
        {
            return Margin<MarginBottom>(control, parentSize);
        }

        public float PaddingLeft(IStyledObject control, float parentSize)
        {
            return Padding<PaddingLeft>(control, parentSize);
        }

        public float PaddingTop(IStyledObject control, float parentSize)
        {
            return Padding<PaddingTop>(control, parentSize);
        }

        public float PaddingRight(IStyledObject control, float parentSize)
        {
            return Padding<PaddingRight>(control, parentSize);
        }

        public float PaddingBottom(IStyledObject control, float parentSize)
        {
            return Padding<PaddingBottom>(control, parentSize);
        }

        public bool HasBorder(IStyledObject control)
        {
            IDictionary<Type, IStyle> styles = _getStyles(control);

            if (styles.ContainsKey(typeof(Border)))
            {
                var border = (Border)styles[typeof(Border)];
                if (border.Style == BorderStyleValues.Solid)
                    return true;
                return false;
            }
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

        public DockAlignValues DockAlign(IStyledObject control)
        {
            IDictionary<Type, IStyle> styles = _getStyles(control);

            if (styles.ContainsKey(typeof(DockAlign)))
            {
                var align = (DockAlign)styles[typeof(DockAlign)];
                return align.Align;
            }
            return DockAlignValues.Top;
        }

        public HorizontalAlignValues HorizontalAlign(IStyledObject control)
        {
            IDictionary<Type, IStyle> styles = _getStyles(control);

            if (styles.ContainsKey(typeof(HorizontalAlign)))
            {
                var align = (HorizontalAlign)styles[typeof(HorizontalAlign)];
                return align.Align;
            }
            return HorizontalAlignValues.Left;
        }

        public VerticalAlignValues VerticalAlign(IStyledObject control)
        {
            IDictionary<Type, IStyle> styles = _getStyles(control);

            if (styles.ContainsKey(typeof(VerticalAlign)))
            {
                var align = (VerticalAlign)styles[typeof(VerticalAlign)];
                return align.Align;
            }
            return VerticalAlignValues.Top;
        }

        public bool SizeToContentWidth(IStyledObject control)
        {
            return SizeToContent<Width>(control);
        }

        public bool SizeToContentHeight(IStyledObject control)
        {
            return SizeToContent<Height>(control);
        }

        private float Size<T>(IStyledObject control, float parentSize, float displayMetric) where T : Size
        {
            float result = 0;

            IDictionary<Type, IStyle> styles = _getStyles(control);

            if (styles.ContainsKey(typeof(T)))
            {
                var size = (Size)styles[typeof(T)];
                result = ConvertSize(size.Measure, size.Amount, parentSize, displayMetric);
            }

            return result;
        }

        private float Margin<T>(IStyledObject control, float parentSize) where T : Margin
        {
            float displayMetric;
            if (typeof(T) == typeof(MarginLeft) || typeof(T) == typeof(MarginRight))
                displayMetric = ApplicationContext.Current.DisplayProvider.Width;
            else if (typeof(T) == typeof(MarginTop) || typeof(T) == typeof(MarginBottom))
                displayMetric = ApplicationContext.Current.DisplayProvider.Height;
            else
                throw new ArgumentException("Type is not Margin type " + typeof(T));

            float r = Size<T>(control, parentSize, displayMetric);
            return r;
        }

        private float Padding<T>(IStyledObject control, float parentSize) where T : Padding
        {
            float displayMetric;
            if (typeof(T) == typeof(PaddingLeft) || typeof(T) == typeof(PaddingRight))
                displayMetric = ApplicationContext.Current.DisplayProvider.Width;
            else if (typeof(T) == typeof(PaddingTop) || typeof(T) == typeof(PaddingBottom))
                displayMetric = ApplicationContext.Current.DisplayProvider.Height;
            else
                throw new ArgumentException("Type is not Padding type " + typeof(T));

            float r = Size<T>(control, parentSize, displayMetric);
            return r;
        }

        private bool SizeToContent<T>(IStyledObject control)
        {
            bool result = false;

            IDictionary<Type, IStyle> styles = _getStyles(control);
            if (styles.ContainsKey(typeof(T)))
            {
                var size = (Size)styles[typeof(T)];
                result = size.SizeToContent;
            }

            return result;
        }
        #endregion

        #region Drawable

        public IColorInfo Color(IStyledObject control)
        {
            return Color<TextColor>(control);
        }

        public IColorInfo BackgroundColor(IStyledObject control)
        {
            return Color<BackgroundColor>(control);
        }

        public IColorInfo SelectedColor(IStyledObject control)
        {
            return Color<SelectedColor>(control);
        }

        public IColorInfo SelectedBackground(IStyledObject control)
        {
            return Color<SelectedBackground>(control);
        }

        public IColorInfo BorderColor(IStyledObject control)
        {
            return Color<BorderColor>(control);
        }

        public IColorInfo PlaceholderColor(IStyledObject control)
        {
            return Color<PlaceholderColor>(control);
        }

        public string BackgroundImage(IStyledObject control)
        {
            IDictionary<Type, IStyle> styles = _getStyles(control);

            if (styles.ContainsKey(typeof(BackgroundImage)))
            {
                var img = (BackgroundImage)styles[typeof(BackgroundImage)];
                return img.Path;
            }
            return null;
        }

        IColorInfo Color<T>(IStyledObject control) where T : Color
        {
            IColorInfo result;

            IDictionary<Type, IStyle> styles = _getStyles(control);

            if (styles.ContainsKey(typeof(T)))
            {
                var color = (Color)styles[typeof(T)];
                result = color.Value;
            }
            else
                result = null;

            return result;
        }

        #endregion

        #region Font

        public float FontSize(IStyledObject control, float parentHeight)
        {
            IDictionary<Type, IStyle> styles = _getStyles(control);

            long currentDepth = 0;
            var measure = Measure.Pixels;
            float size = 0;

            if (styles.ContainsKey(typeof(Font)))
            {
                var font = (Font)styles[typeof(Font)];
                currentDepth = font.Depth;
                measure = font.Measure;
                size = font.Value;
            }

            if (styles.ContainsKey(typeof(FontSize)))
            {
                var fontSize = (FontSize)styles[typeof(FontSize)];
                if (fontSize.Depth > currentDepth)
                {
                    measure = fontSize.Measure;
                    size = fontSize.Size;
                }
            }

            float result = ConvertSize(measure, size, parentHeight, ApplicationContext.Current.DisplayProvider.Height);
            return (int)Math.Round(result);
        }

        public string FontName(IStyledObject control)
        {
            IDictionary<Type, IStyle> styles = _getStyles(control);

            long currentDepth = 0;
            string result = null;
            if (styles.ContainsKey(typeof(Font)))
            {
                var font = (Font)styles[typeof(Font)];
                currentDepth = font.Depth;
                result = font.Family;
            }

            if (styles.ContainsKey(typeof(FontFamily)))
            {
                var fontFamily = (FontFamily)styles[typeof(FontFamily)];
                if (fontFamily.Depth > currentDepth)
                    result = fontFamily.Family;
            }

            return result;
        }

        public TextAlignValues TextAlign(IStyledObject control, TextAlignValues defaultValue = TextAlignValues.Left)
        {
            IDictionary<Type, IStyle> styles = _getStyles(control);

            if (styles.ContainsKey(typeof(TextAlign)))
            {
                var align = (TextAlign)styles[typeof(TextAlign)];
                return align.Align;
            }
            return defaultValue;
        }

        public TextFormatValues TextFormat(IStyledObject control, TextFormatValues defaulValue = TextFormatValues.Text)
        {
            IDictionary<Type, IStyle> styles = _getStyles(control);

            if (styles.ContainsKey(typeof(TextFormat)))
            {
                var format = (TextFormat)styles[typeof(TextFormat)];
                return format.Format;
            }
            return defaulValue;
        }

        public WhiteSpaceKind WhiteSpace(IStyledObject control, WhiteSpaceKind defaultValue = WhiteSpaceKind.Nowrap)
        {
            IDictionary<Type, IStyle> styles = _getStyles(control);

            if (styles.ContainsKey(typeof(WhiteSpace)))
            {
                var ws = (WhiteSpace)styles[typeof(WhiteSpace)];
                return ws.Kind;
            }
            return defaultValue;
        }
        #endregion

        private static float ConvertSize(Measure measure, float value, float parentSize, float displayMetric)
        {
            float result;
            switch (measure)
            {
                case Measure.Pixels:
                    result = value / Application.StyleSheet.StyleSheetContext.Current.Scale;
                    break;
                case Measure.Percent:
                    result = value * parentSize / 100;
                    break;
                case Measure.ScreenPercent:
                    result = value * displayMetric / 100;
                    break;
                case Measure.Millimetre:
                    double px = value * ApplicationContext.Current.DisplayProvider.PxPerMm;
                    result = (int)Math.Round(px);
                    break;
                case Measure.Dip:
                    const float maxWidth = 980;
                    float coeff = ApplicationContext.Current.DisplayProvider.Width / maxWidth;
                    result = value * coeff;
                    break;
                default:
                    result = value;
                    break;
            }
            return result;
        }
    }
}
