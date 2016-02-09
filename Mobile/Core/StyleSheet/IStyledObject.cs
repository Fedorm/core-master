using System;

namespace BitMobile.Controls.StyleSheet
{
    public interface IStyledObject
    {
        String CssClass { get; set; }
        Bound Apply(StyleSheet stylesheet, Bound styleBound, Bound maxBound);
    }
}

