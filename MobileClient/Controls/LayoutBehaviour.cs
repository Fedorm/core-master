using System;
using System.Collections.Generic;
using System.Linq;
using BitMobile.Application.StyleSheet;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;

namespace BitMobile.Controls
{
    public class LayoutBehaviour : ILayoutBehaviour
    {
        private readonly IStyleSheet _stylesheet;
        private readonly ILayoutable _container;

        public LayoutBehaviour(IStyleSheet stylesheet, ILayoutable container)
        {
            _stylesheet = stylesheet;
            _container = container;
        }

        public IBound Horizontal(IEnumerable<ILayoutable> controls, IBound styleBound, IBound maxBound, out float[] borders, bool extensible = false)
        {
            return Queue(controls, styleBound, maxBound, out borders, true, extensible);
        }

        public IBound Vertical(IEnumerable<ILayoutable> controls, IBound styleBound, IBound maxBound, out float[] borders, bool extensible = false)
        {
            return Queue(controls, styleBound, maxBound, out borders, false, extensible);
        }

        public IBound Dock(IEnumerable<ILayoutable> controls, IBound styleBound, IBound maxBound)
        {
            IStyleSheetHelper style = _stylesheet.Helper;
            IStyleSheetContext context = StyleSheetContext.Current;

            float parentW = styleBound.Width;
            float parentH = styleBound.Height;
            float paddingL = style.PaddingLeft(_container, parentW);
            float paddingT = style.PaddingTop(_container, parentH);
            float paddingR = style.PaddingRight(_container, parentW);
            float paddingB = style.PaddingBottom(_container, parentH);
            float borderWidth = style.BorderWidth(_container);

            float left = borderWidth + paddingL;
            float top = borderWidth + paddingT;
            float right = parentW - (paddingR + borderWidth);
            float bottom = parentH - (paddingB + borderWidth);

            float resizedWidth = parentW;
            float resizedHeight = parentH;

            float freeW = style.SizeToContentWidth(_container) ? maxBound.Width - parentW : 0;
            float freeH = style.SizeToContentHeight(_container) ? maxBound.Height - parentH : 0;

            IList<ILayoutable> controlsList = controls as IList<ILayoutable> ?? controls.ToList();
            var frames = new Rectangle[controlsList.Count];
            for (int i = 0; i < controlsList.Count; i++)
            {
                ILayoutable control = controlsList[i];

                float w = style.Width(control, parentW);
                float h = style.Height(control, parentH);

                float marginL = style.MarginLeft(control, parentW);
                float marginT = style.MarginTop(control, parentH);
                float marginR = style.MarginRight(control, parentW);
                float marginB = style.MarginBottom(control, parentH);

                float maxW = right - left - (marginL + marginR);
                float maxH = bottom - top - (marginT + marginB);

                resizedWidth += SizeTo(false, ref w, ref maxW, ref freeW, ref right);
                resizedHeight += SizeTo(false, ref h, ref maxH, ref freeH, ref bottom);

                IBound bound = control.ApplyStyles(_stylesheet, context.CreateBound(w, h)
                    , context.CreateBound(maxW + freeW, maxH + freeH));
                w = bound.Width;
                h = bound.Height;

                resizedWidth += SizeTo(false, ref w, ref maxW, ref freeW, ref right);
                resizedHeight += SizeTo(false, ref h, ref maxH, ref freeH, ref bottom);

                float x = left + marginL;
                float y = top + marginT;

                DockAlignValues align = style.DockAlign(control);
                switch (align)
                {
                    case DockAlignValues.Left:
                        left += marginL + w + marginR;
                        break;
                    case DockAlignValues.Top:
                        top += marginT + h + marginB;
                        break;
                    case DockAlignValues.Right:
                        x = right - (marginR + w);
                        right -= marginL + w + marginR;

                        // reverse reference system, for size to content
                        x = -(resizedWidth - x);

                        break;
                    case DockAlignValues.Bottom:
                        y = bottom - (marginB + h);
                        bottom -= marginT + h + marginB;

                        // reverse reference system, for size to content
                        y = -(resizedHeight - y);

                        break;
                    default:
                        throw new Exception("Unknown align: " + align);
                }

                frames[i] = new Rectangle(x, y, w, h);
            }

            // restore reference system
            for (int i = 0; i < controlsList.Count; i++)
            {
                Rectangle f = frames[i];
                float x = f.Left >= 0 ? f.Left : resizedWidth + f.Left;
                float y = f.Top >= 0 ? f.Top : resizedHeight + f.Top;
                controlsList[i].Frame = new Rectangle(x, y, f.Width, f.Height);
            }

            return context.CreateBound(resizedWidth, resizedHeight);
        }

        public IBound Screen(ILayoutable control, IBound screenBound)
        {
            IStyleSheetHelper style = _stylesheet.Helper;
            IStyleSheetContext context = StyleSheetContext.Current;

            float parentW = screenBound.Width;
            float parentH = screenBound.Height;
            float paddingL = style.PaddingLeft(_container, parentW);
            float paddingT = style.PaddingTop(_container, parentH);
            float paddingR = style.PaddingRight(_container, parentW);
            float paddingB = style.PaddingBottom(_container, parentH);
            float borderWidth = style.BorderWidth(_container);

            float w = style.Width(control, parentW);
            float h = style.Height(control, parentH);
            float marginL = style.MarginLeft(control, parentW);
            float marginT = style.MarginTop(control, parentH);
            float marginR = style.MarginRight(control, parentW);
            float marginB = style.MarginBottom(control, parentH);

            float maxW = parentW - (marginL + marginR) - (paddingL + paddingR) - 2 * borderWidth;
            if (parentW < paddingL + marginL + w + marginR + paddingR + 2 * borderWidth)
                w = maxW;

            float maxH = parentH - (marginT + marginB) - (paddingT + paddingB) - 2 * borderWidth;
            if (parentH < paddingT + marginT + h + marginB + paddingB + 2 * borderWidth)
                h = maxH;

            float l = borderWidth + paddingL + marginL;
            float t = borderWidth + paddingT + marginT;

            IBound bound = control.ApplyStyles(_stylesheet, context.CreateBound(w, h), context.CreateBound(maxW, maxH));
            w = bound.Width > maxW ? maxW : bound.Width;
            h = bound.Height > maxH ? maxH : bound.Height;

            control.Frame = new Rectangle(l, t, w, h);

            return screenBound;
        }

        private IBound Queue(IEnumerable<ILayoutable> controls, IBound styleBound, IBound maxBound, out float[] borders, bool horizontal, bool extensible)
        {
            if (styleBound.Width > maxBound.Width || styleBound.Height > maxBound.Height)
                throw new ArgumentException("maxBoud lesser than styleBound");

            IStyleSheetHelper style = _stylesheet.Helper;
            IStyleSheetContext context = StyleSheetContext.Current;
            var bordersList = new List<float>();

            float parentW = styleBound.Width;
            float parentH = styleBound.Height;
            float paddingL = style.PaddingLeft(_container, parentW);
            float paddingT = style.PaddingTop(_container, parentH);
            float paddingR = style.PaddingRight(_container, parentW);
            float paddingB = style.PaddingBottom(_container, parentH);
            float borderWidth = style.BorderWidth(_container);

            float left = paddingL + borderWidth;
            float top = paddingT + borderWidth;
            float right = parentW - (borderWidth + paddingR);
            float bottom = parentH - (borderWidth + paddingB);

            float resizedWidth = parentW;
            float resizedHeight = parentH;

            bool sizeToContentWidth = style.SizeToContentWidth(_container);
            bool sizeToContentHeight = style.SizeToContentHeight(_container);

            float freeW = sizeToContentWidth ? maxBound.Width - parentW : 0;
            float freeH = sizeToContentHeight ? maxBound.Height - parentH : 0;

            bordersList.Add(horizontal ? left : top);

            IList<ILayoutable> controlsList = controls as IList<ILayoutable> ?? controls.ToList();
            var dirtyFrames = new LayoutParams[controlsList.Count];
            for (int i = 0; i < controlsList.Count; i++)
            {
                ILayoutable control = controlsList[i];
                float w = style.Width(control, parentW);
                float h = style.Height(control, parentH);

                float marginL = style.MarginLeft(control, parentW);
                float marginT = style.MarginTop(control, parentH);
                float marginR = style.MarginRight(control, parentW);
                float marginB = style.MarginBottom(control, parentH);

                float maxW = right - left - (marginL + marginR);
                float maxH = bottom - top - (marginT + marginB);

                resizedWidth += SizeTo(horizontal && extensible, ref w, ref maxW, ref freeW, ref right);
                resizedHeight += SizeTo(!horizontal && extensible, ref h, ref maxH, ref freeH, ref bottom);

                float childMaxW = horizontal && extensible ? float.MaxValue : maxW + freeW;
                float childMaxH = !horizontal && extensible ? float.MaxValue : maxH + freeH;
                IBound bound = control.ApplyStyles(_stylesheet, context.CreateBound(w, h), context.CreateBound(childMaxW, childMaxH));
                w = bound.Width;
                h = bound.Height;

                resizedWidth += SizeTo(horizontal && extensible, ref w, ref maxW, ref freeW, ref right);
                resizedHeight += SizeTo(!horizontal && extensible, ref h, ref maxH, ref freeH, ref bottom);

                dirtyFrames[i] = new LayoutParams(left, top, w, h, marginL, marginT, marginR, marginB); // new Rectangle(left + leftOffset, top + topOffset, w, h);

                if (horizontal)
                    left += marginL + w + marginR;
                else
                    top += marginT + h + marginB;

                bordersList.Add(horizontal ? left : top);
            }

            borders = bordersList.ToArray();

            for (int i = 0; i < controlsList.Count; i++)
            {
                ILayoutable control = controlsList[i];
                LayoutParams layoutParams = dirtyFrames[i];

                float topOffset = layoutParams.MarginTop;
                float leftOffset = layoutParams.MarginLeft;
                if (horizontal)
                {
                    float maxH = bottom - top - (layoutParams.MarginTop + layoutParams.MarginBottom);
                    if (layoutParams.Height < maxH)
                    {
                        VerticalAlignValues align = style.VerticalAlign(control);
                        switch (align)
                        {
                            case VerticalAlignValues.Top:
                                break;
                            case VerticalAlignValues.Center:
                            case VerticalAlignValues.Central:
                                topOffset += (maxH - layoutParams.Height) / 2;
                                break;
                            case VerticalAlignValues.Bottom:
                                topOffset = (bottom - layoutParams.MarginBottom - layoutParams.Height) - top;
                                break;
                        }
                    }
                }
                else
                {
                    float maxW = right - left - (layoutParams.MarginLeft + layoutParams.MarginRight);
                    if (layoutParams.Width < maxW)
                    {
                        HorizontalAlignValues align = style.HorizontalAlign(control);
                        switch (align)
                        {
                            case HorizontalAlignValues.Left:
                                break;
                            case HorizontalAlignValues.Center:
                            case HorizontalAlignValues.Central:
                                leftOffset += (maxW - layoutParams.Width) / 2;
                                break;
                            case HorizontalAlignValues.Right:
                                leftOffset = (right - layoutParams.MarginRight - layoutParams.Width) - left;
                                break;
                        }
                    }
                }

                control.Frame = new Rectangle(layoutParams.Left + leftOffset, layoutParams.Top + topOffset
                    , layoutParams.Width, layoutParams.Height);
            }
            
            float contentW = horizontal ? left + paddingR + borderWidth : resizedWidth;
            float contentH = !horizontal ? top + paddingB + borderWidth : resizedHeight;
            return context.CreateBound(resizedWidth, resizedHeight, contentW, contentH);
        }

        /// <summary>
        /// Changing parameters for size to content
        /// </summary>
        /// <param name="extensible">Child in swipe layout</param>
        /// <param name="size">Widht/Height of child</param>
        /// <param name="max">Maximum free space without streching</param>
        /// <param name="free">Available additional free space</param>
        /// <param name="end">Right/Bottom coordinate of parent with paddings</param>
        /// <returns></returns>
        private static float SizeTo(bool extensible, ref float size, ref float max, ref float free, ref float end)
        {
            if (size > max)
            {
                float delta = Math.Min(size - max, free);
                free -= delta;
                end += delta;
                max += delta;
                if (!extensible && Math.Abs(free) < 0.001)
                    size = max;
                return delta;
            }
            return 0;
        }

        struct LayoutParams
        {
            public LayoutParams(float left, float top, float width, float height
                , float marginLeft, float marginTop, float marginRight, float marginBottom)
                : this()
            {
                Left = left;
                Top = top;
                Width = width;
                Height = height;
                MarginLeft = marginLeft;
                MarginTop = marginTop;
                MarginRight = marginRight;
                MarginBottom = marginBottom;
            }

            public float Left { get; private set; }
            public float Top { get; private set; }
            public float Width { get; private set; }
            public float Height { get; private set; }

            public float MarginLeft { get; private set; }
            public float MarginTop { get; private set; }
            public float MarginRight { get; private set; }
            public float MarginBottom { get; private set; }
        }
    }
}