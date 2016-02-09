using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitMobile.Controls.StyleSheet
{
    public interface IStyleSheetHelper
    {
        #region Layout

        float Width(IStyledObject control, float parentSize);
        
        float Height(IStyledObject control, float parentSize);

        float Margin<T>(IStyledObject control, float parentSize) where T : Margin;

        float Padding<T>(IStyledObject control, float parentSize) where T : Padding;

        bool HasBorder(IStyledObject control);

        float BorderWidth(IStyledObject control);

        float BorderRadius(IStyledObject control);

        DockAlign.Align DockAlign(IStyledObject control);

        HorizontalAlign.Align HorizontalAlign(IStyledObject control);

        VerticalAlign.Align VerticalAlign(IStyledObject control);

        bool SizeToContentWidth(IStyledObject control);

        bool SizeToContentHeight(IStyledObject control);
        #endregion
    }
}