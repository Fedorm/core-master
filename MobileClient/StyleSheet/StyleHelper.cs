using System;
using System.Collections.Generic;
using System.Linq;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    struct StyleHelper : IStyleHelper
    {
        private readonly IDictionary<Type, IStyle> _styles;
        private readonly IStyleSheet _styleSheet;
        private readonly IStyledObject _subject;

        public StyleHelper(IDictionary<Type, IStyle> styles, IStyleSheet styleSheet, IStyledObject subject)
        {
            _styles = styles;
            _styleSheet = styleSheet;
            _subject = subject;
        }

        public T Get<T>() where T : IStyle
        {
            T style;
            TryGet(out style);
            return style;
        }

        public bool TryGet<T>(out T style) where T : IStyle
        {
            style = (T)_styles.Values.FirstOrDefault(val => val is T);
            if (ReferenceEquals(null, style))
            {
                style = (T)_styleSheet.GetStyles(_subject).Values.FirstOrDefault(val => val is T);
                if (ReferenceEquals(null, style))
                    style = (T)DefaultStyles[typeof(T)];
                return false;
            }
            return true;
        }

        public bool Exsists<T>() where T : IStyle
        {
            bool exsists = _styles.Values.Count(val => val is T) > 0;
            if (!exsists)
                exsists = _styleSheet.GetStyles(_subject).Values.Count(val => val is T) > 0;
            return exsists;
        }

        public bool TryGetFontSize(float parentSize, out float size)
        {
            size = Size.DefaultAmount;
            long currentDepth = -1;
            IFont font;
            if (TryGet(out font))
            {
                size = font.CalcSize(parentSize);
                currentDepth = font.Depth;
            }

            IFontSize fontSize;
            if (TryGet(out fontSize))
            {
                if (fontSize.Depth > currentDepth)
                {
                    size = fontSize.CalcSize(parentSize);
                    currentDepth = fontSize.Depth;
                }
            }
            return currentDepth >= 0;
        }

        public bool TryGetFontFamily(out string family)
        {
            family = Font.DefaultFontFamily;
            long currentDepth = -1;
            IFont font;
            if (TryGet(out font))
            {
                family = font.Family;
                currentDepth = font.Depth;
            }

            IFontFamily fontFamily;
            if (TryGet(out fontFamily))
            {
                if (fontFamily.Depth > currentDepth)
                {
                    family = fontFamily.Family;
                    currentDepth = fontFamily.Depth;
                }
            }
            return currentDepth >= 0;
        }

        public float GetSizeOrDefault<T>(float parentSize, int defaultValue) where T : ISize
        {
            T size;
            if (TryGet(out size))
                return size.CalcSize(parentSize, size.DisplayMetric);

            return defaultValue;
        }

        #region Default Styles

        private readonly static IDictionary<Type, IStyle> DefaultStyles = new Dictionary<Type, IStyle>
        {
            { typeof(IBackgroundColor), new BackgroundColor(0) },
            { typeof(IBackgroundImage),  new BackgroundImage(0) },
            { typeof(IBorderColor), new BorderColor(0) },
            { typeof(IBorderRadius), new BorderRadius(0) },
            { typeof(IBorderStyle), new Border(0) },
            { typeof(IBorderWidth), new BorderWidth(0) },
            { typeof(ITextColor), new TextColor(0) },
            { typeof(IDockAlign), new DockAlign(0) },
            { typeof(IFont), new Font(0) },
            { typeof(IFontFamily), new FontFamily(0) },
            { typeof(IFontSize), new FontSize(0) },
            { typeof(IHeight), new Height(0) },
            { typeof(IHorizontalAlign), new HorizontalAlign(0) },
            { typeof(IMarginLeft), new MarginLeft(0) },
            { typeof(IMarginTop), new MarginTop(0) },
            { typeof(IMarginRight), new MarginRight(0) },
            { typeof(IMarginBottom), new MarginBottom(0) },
            { typeof(IPaddingLeft), new PaddingLeft(0) },
            { typeof(IPaddingTop), new PaddingTop(0) },
            { typeof(IPaddingRight), new PaddingRight(0) },
            { typeof(IPaddingBottom), new PaddingBottom(0) },
            { typeof(IPlaceholderColor), new PlaceholderColor(0) },
            { typeof(ISelectedBackground), new SelectedBackground(0) },
            { typeof(ISelectedColor), new SelectedColor(0) },
            { typeof(ITextAlign), new TextAlign(0) },
            { typeof(ITextFormat), new TextFormat(0) },
            { typeof(IVerticalAlign), new VerticalAlign(0) },
            { typeof(IWhiteSpace), new WhiteSpace(0) },
            { typeof(IWidth), new Width(0)}
        };
        #endregion
    }
}