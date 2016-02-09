namespace BitMobile.Common.StyleSheet
{
    #region Size

    public interface ISize : IStyle
    {
        float DisplayMetric { get; }
        float CalcSize(float parentSize, float displayMetric);
    }

    public interface IHeight : ISize
    {
        bool SizeToContent { get; }
    }

    public interface IWidth : ISize
    {
        bool SizeToContent { get; }
    }

    public interface IMarginLeft : ISize { }

    public interface IMarginTop : ISize { }

    public interface IMarginRight : ISize { }

    public interface IMarginBottom : ISize { }

    public interface IPaddingLeft : ISize { }

    public interface IPaddingTop : ISize { }

    public interface IPaddingRight : ISize { }

    public interface IPaddingBottom : ISize { }

    #endregion

    #region Layout

    public interface IDockAlign : IStyle
    {
        DockAlignValues Align { get; }
    }

    public interface IHorizontalAlign : IStyle
    {
        HorizontalAlignValues Align { get; }
    }

    public interface IVerticalAlign : IStyle
    {
        VerticalAlignValues Align { get; }
    }

    #endregion

    #region Drawable

    public interface IBackgroundImage : IStyle
    {
        string Path { get; }
    }

    public interface IColor : IStyle
    {
        IColorInfo Value { get; }
    }

    public interface ITextColor : IColor { }

    public interface IBackgroundColor : IColor { }

    public interface IPlaceholderColor : IColor { }

    public interface ISelectedBackground : IColor { }

    public interface ISelectedColor : IColor { }

    #endregion

    #region Borders

    public interface IBorderStyle : IStyle
    {
        BorderStyleValues Style { get; }
    }

    public interface IBorderColor : IColor { }

    public interface IBorderRadius : IStyle
    {
        float Radius { get; }
    }

    public interface IBorderWidth : IStyle
    {
        float Value { get; }
    }

    #endregion

    #region Format

    public interface IFont : IStyle
    {
        string Family { get; }
        float CalcSize(float parentSize);
    }

    public interface IFontSize : IStyle
    {
        float CalcSize(float parentSize);
    }

    public interface IFontFamily : IStyle
    {
        string Family { get; }
    }

    public interface ITextFormat : IStyle
    {
        TextFormatValues Format { get; }
    }

    public interface ITextAlign : IStyle
    {
        TextAlignValues Align { get; }
    }

    public interface IWhiteSpace : IStyle
    {
        WhiteSpaceKind Kind { get; }
    }

    #endregion
}