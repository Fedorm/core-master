using System;

namespace BitMobile.Common.StyleSheet
{
    public interface IStyleSheetHelper
    {
        #region Layout

        float Width(IStyledObject control, float parentSize);
        float Height(IStyledObject control, float parentSize);
        float MarginLeft(IStyledObject control, float parentSize);
        float MarginTop(IStyledObject control, float parentSize);
        float MarginRight(IStyledObject control, float parentSize);
        float MarginBottom(IStyledObject control, float parentSize);
        float PaddingLeft(IStyledObject control, float parentSize);
        float PaddingTop(IStyledObject control, float parentSize);
        float PaddingRight(IStyledObject control, float parentSize);
        float PaddingBottom(IStyledObject control, float parentSize);
        bool HasBorder(IStyledObject control);
        float BorderWidth(IStyledObject control);
        float BorderRadius(IStyledObject control);
        DockAlignValues DockAlign(IStyledObject control);
        HorizontalAlignValues HorizontalAlign(IStyledObject control);
        VerticalAlignValues VerticalAlign(IStyledObject control);
        bool SizeToContentWidth(IStyledObject control);
        bool SizeToContentHeight(IStyledObject control);
        #endregion

        #region Drawable

        IColorInfo Color(IStyledObject control);
        IColorInfo BackgroundColor(IStyledObject control);
        IColorInfo SelectedColor(IStyledObject control);
        IColorInfo SelectedBackground(IStyledObject control);
        IColorInfo BorderColor(IStyledObject control);
        IColorInfo PlaceholderColor(IStyledObject control);
        string BackgroundImage(IStyledObject control);
        #endregion

        #region Font

        float FontSize(IStyledObject control, float parentHeight);
        string FontName(IStyledObject control);
        TextAlignValues TextAlign(IStyledObject control, TextAlignValues defaultValue = TextAlignValues.Left);
        TextFormatValues TextFormat(IStyledObject control, TextFormatValues defaulValue = TextFormatValues.Text);
        WhiteSpaceKind WhiteSpace(IStyledObject control, WhiteSpaceKind defaultValue = WhiteSpaceKind.Nowrap);
        #endregion
    }
}