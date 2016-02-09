using System;
using System.Collections.Generic;
using BitMobile.Controls.StyleSheet;

namespace BitMobile.Controls
{
    public static class LayoutBehaviour
    {
        public static Bound Horizontal(StyleSheet.StyleSheet stylesheet
            , ILayoutable parent
            , IEnumerable<ILayoutable> controls
            , Bound styleBound
            , Bound maxBound
            , bool extensible = false)
        {
            IStyleSheetHelper style = stylesheet.Helper;

            float parentW = styleBound.Width;
            float parentH = styleBound.Height;
            float paddingL = style.Padding<PaddingLeft>(parent, parentW);
            float paddingT = style.Padding<PaddingTop>(parent, parentH);
            float paddingR = style.Padding<PaddingRight>(parent, parentW);
            float paddingB = style.Padding<PaddingBottom>(parent, parentH);
            float borderWidth = style.BorderWidth(parent);

            float off = paddingL + borderWidth;

            foreach (ILayoutable control in controls)
            {
                float w = style.Width(control, parentW);
                float h = style.Height(control, parentH);

                float marginL = style.Margin<MarginLeft>(control, parentW);
                float marginT = style.Margin<MarginTop>(control, parentH);
                float marginR = style.Margin<MarginRight>(control, parentW);
                float marginB = style.Margin<MarginBottom>(control, parentH);

                off += marginL;

                float left = off;
                float top = paddingT + marginT + borderWidth;
                float right = parentW - (borderWidth + paddingR + marginR);
                float bottom = parentH - (borderWidth + paddingB + marginB);

                float maxW = extensible ? float.MaxValue : right - left;
                maxW = maxW > 0 ? maxW : 0;

                float maxH = bottom - top;
                maxH = maxH > 0 ? maxH : 0;

                if (!extensible && maxW < w)
                    w = maxW;

                if (h > maxH)
                    h = maxH;

                Bound bound = control.Apply(stylesheet, new Bound(w, h), new Bound(maxW, maxH));
                w = bound.Width;
                h = bound.Height;

                InitializeImageContainer(stylesheet, control, ref w, ref h);

                if (!extensible && maxW < w)
                    w = maxW;

                if (h < maxH)
                {
                    VerticalAlign.Align align = style.VerticalAlign(control);
                    switch (align)
                    {
                        case VerticalAlign.Align.Top:
                            break;
                        case VerticalAlign.Align.Central:
                            float delta = (bottom - top - h) / 2;
                            top += delta;
                            break;
                        case VerticalAlign.Align.Bottom:
                            top = bottom - h;
                            break;
                    }
                }

                control.Frame = new Rectangle(left, top, w, h);

                off += w + marginR;
            }

            return new Bound(styleBound.Width, styleBound.Height, off + paddingR + borderWidth, parentH);
        }

        public static Bound Vertical(StyleSheet.StyleSheet stylesheet
            , ILayoutable parent
            , IEnumerable<ILayoutable> controls
            , Bound styleBound
            , Bound maxBound
            , bool extensible = false)
        {
            IStyleSheetHelper style = stylesheet.Helper;

            float parentW = styleBound.Width;
            float parentH = styleBound.Height;
            float paddingL = style.Padding<PaddingLeft>(parent, parentW);
            float paddingT = style.Padding<PaddingTop>(parent, parentH);
            float paddingR = style.Padding<PaddingRight>(parent, parentW);
            float paddingB = style.Padding<PaddingBottom>(parent, parentH);
            float borderWidth = style.BorderWidth(parent);

            float off = paddingT + borderWidth;

            foreach (ILayoutable control in controls)
            {
                float w = style.Width(control, parentW);
                float h = style.Height(control, parentH);

                float marginL = style.Margin<MarginLeft>(control, parentW);
                float marginT = style.Margin<MarginTop>(control, parentH);
                float marginR = style.Margin<MarginRight>(control, parentW);
                float marginB = style.Margin<MarginBottom>(control, parentH);

                off += marginT;

                float left = borderWidth + paddingL + marginL;
                float top = off;
                float right = parentW - (paddingR + marginR) - borderWidth;
                float bottom = parentH - (paddingB + marginB) - borderWidth;

                float maxW = right - left;
                maxW = maxW > 0 ? maxW : 0;

                float maxH = extensible ? float.MaxValue : bottom - top;
                maxH = maxH > 0 ? maxH : 0;

                if (!extensible && maxH < h)
                    h = maxH;

                w = w > maxW ? maxW : w;

                Bound bound = control.Apply(stylesheet, new Bound(w, h), new Bound(maxW, maxH));
                w = bound.Width;
                h = bound.Height;

                InitializeImageContainer(stylesheet, control, ref w, ref h);

                if (!extensible && maxH < h)
                    h = maxH;

                if (w < maxW)
                {
                    HorizontalAlign.Align align = style.HorizontalAlign(control);
                    switch (align)
                    {
                        case HorizontalAlign.Align.Left:
                            break;
                        case HorizontalAlign.Align.Central:
                            float delta = (right - left - w) / 2;
                            left += delta;
                            break;
                        case HorizontalAlign.Align.Right:
                            left = right - w;
                            break;
                    }
                }

                control.Frame = new Rectangle(left, top, w, h);

                off += h + marginB;
            }

            return new Bound(styleBound.Width, styleBound.Height, parentW, off + paddingB + borderWidth);
        }

        public static Bound Dock(StyleSheet.StyleSheet stylesheet
            , ILayoutable parent
            , IEnumerable<ILayoutable> controls
            , Bound styleBound
            , Bound maxBound)
        {
            IStyleSheetHelper style = stylesheet.Helper;

            float parentW = styleBound.Width;
            float parentH = styleBound.Height;
            float paddingL = style.Padding<PaddingLeft>(parent, parentW);
            float paddingT = style.Padding<PaddingTop>(parent, parentH);
            float paddingR = style.Padding<PaddingRight>(parent, parentW);
            float paddingB = style.Padding<PaddingBottom>(parent, parentH);
            float borderWidth = style.BorderWidth(parent);

            float layoutL = borderWidth + paddingL;
            float layoutT = borderWidth + paddingT;
            float layoutW = parentW - (paddingL + paddingR) - 2 * borderWidth;
            float layoutH = parentH - (paddingT + paddingB) - 2 * borderWidth;

            foreach (ILayoutable control in controls)
            {
                float w = style.Width(control, parentW);
                float h = style.Height(control, parentH);

                float marginL = style.Margin<MarginLeft>(control, parentW);
                float marginT = style.Margin<MarginTop>(control, parentH);
                float marginR = style.Margin<MarginRight>(control, parentW);
                float marginB = style.Margin<MarginBottom>(control, parentH);

                float maxW = layoutW - (marginL + marginR);
                maxW = maxW > 0 ? maxW : 0;

                float maxH = layoutH - (marginT + marginB);
                maxH = maxH > 0 ? maxH : 0;

                if (layoutW < marginL + w + marginR)
                    w = maxW;
                if (layoutH < marginT + h + marginB)
                    h = maxH;

                Bound bound = control.Apply(stylesheet, new Bound(w, h), new Bound(maxW, maxH));
                w = bound.Width;
                h = bound.Height;

                InitializeImageContainer(stylesheet, control, ref w, ref h);

                if (layoutW < marginL + w + marginR)
                    w = maxW;
                if (layoutH < marginT + h + marginB)
                    h = maxH;

                float offsetL = layoutL;
                float offsetT = layoutT;

                DockAlign.Align align = style.DockAlign(control);
                switch (align)
                {
                    case DockAlign.Align.Left:
                        offsetL += marginL;
                        offsetT += marginT;
                        layoutL = offsetL + w + marginR;
                        layoutW -= marginL + w + marginR;
                        break;
                    case DockAlign.Align.Top:
                        offsetL += marginL;
                        offsetT += marginT;
                        layoutT = offsetT + h + marginB;
                        layoutH -= marginT + h + marginB;
                        break;
                    case DockAlign.Align.Right:
                        offsetL += layoutW - (w + marginR);
                        offsetT += marginT;
                        layoutW -= marginL + w + marginR;
                        break;
                    case DockAlign.Align.Bottom:
                        offsetL += marginL;
                        offsetT += layoutH - (h + marginB);
                        layoutH -= marginT + h + marginB;
                        break;
                }
                control.Frame = new Rectangle(offsetL, offsetT, w, h);
            }

            return styleBound;
        }

        public static Bound Screen(StyleSheet.StyleSheet stylesheet
            , ILayoutable parent
            , ILayoutable control
            , Bound screenBound)
        {
            IStyleSheetHelper style = stylesheet.Helper;

            float parentW = screenBound.Width;
            float parentH = screenBound.Height;
            float paddingL = style.Padding<PaddingLeft>(parent, parentW);
            float paddingT = style.Padding<PaddingTop>(parent, parentH);
            float paddingR = style.Padding<PaddingRight>(parent, parentW);
            float paddingB = style.Padding<PaddingBottom>(parent, parentH);
            float borderWidth = style.BorderWidth(parent);

            float w = style.Width(control, parentW);
            float h = style.Height(control, parentH);
            float marginL = style.Margin<MarginLeft>(control, parentW);
            float marginT = style.Margin<MarginTop>(control, parentH);
            float marginR = style.Margin<MarginRight>(control, parentW);
            float marginB = style.Margin<MarginBottom>(control, parentH);

            InitializeImageContainer(stylesheet, control, ref w, ref h);

            float maxW = parentW - (marginL + marginR) - (paddingL + paddingR) - 2 * borderWidth;
            if (parentW < paddingL + marginL + w + marginR + paddingR + 2 * borderWidth)
                w = maxW;

            float maxH = parentH - (marginT + marginB) - (paddingT + paddingB) - 2 * borderWidth;
            if (parentH < paddingT + marginT + h + marginB + paddingB + 2 * borderWidth)
                h = maxH;

            float l = borderWidth + paddingL + marginL;
            float t = borderWidth + paddingT + marginT;

            Bound bound = control.Apply(stylesheet, new Bound(w, h), new Bound(maxW, maxH));
            w = bound.Width > maxW ? maxW : bound.Width;
            h = bound.Height > maxH ? maxH : bound.Height;

            control.Frame = new Rectangle(l, t, w, h);
            return screenBound;
        }

        public static void InitializeImageContainer(StyleSheet.StyleSheet stylesheet
              , ILayoutable control
              , ref float w
              , ref float h)
        {
            var container = control as IImageContainer;
            if (container != null && container.InitializeImage(stylesheet))
            {
                float proportion = ((float)container.ImageWidth) / container.ImageHeight;
                if (w == 0 && h != 0)
                    w = (int)Math.Round(h * proportion);
                else if (h == 0 && w != 0)
                    h = (int)Math.Round(w / proportion);
            }
        }
    }
}