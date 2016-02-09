using System;
using System.Collections.Generic;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    public class StyleSheetContext : IStyleSheetContext
    {
        public StyleSheetContext()
        {
            Scale = 1;
        }

        public float Scale { get; set; }

        public IBound EmptyBound
        {
            get { return new Bound(); }
        }

        public IStyleSheet CreateStyleSheet()
        {
            return new StyleSheet();
        }

        public IBound CreateBound(float width, float height)
        {
            return new Bound(width, height);
        }

        public IBound CreateBound(float width, float height, float contentWidth, float contentHeight)
        {
            return new Bound(width, height, contentWidth, contentHeight);
        }

        public IBound MergeBound(IBound bound, float width, float height, IBound maxBound, bool safeProportion)
        {
            float w = Math.Max(width, bound.Width);
            float h = Math.Max(height, bound.Height);

            if (safeProportion)
            {
                float proportion = bound.Width / bound.Height;
                float newHeight = Convert.ToSingle(Math.Round(w / proportion));
                if (newHeight >= h)
                    h = newHeight;
                else
                    w = Convert.ToSingle(Math.Round(h * proportion));
            }

            return CreateBound(Math.Min(w, maxBound.Width), Math.Min(h, maxBound.Height));
        }

        public IBound StrechBoundInProportion(IBound styleBound, IBound maxBound, float widthProportion, float heightProportion)
        {
            float proportion = widthProportion / heightProportion;
            float w = styleBound.Width;
            float h = styleBound.Height;

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (w == 0 && h != 0)
            {
                w = Convert.ToSingle(Math.Round(h * proportion));
                if (w > maxBound.Width)
                {
                    w = maxBound.Width;
                    h = Convert.ToSingle(Math.Round(w / proportion));
                }
            }
            else if (h == 0 && w != 0)
            {
                h = Convert.ToSingle(Math.Round(w / proportion));
                if (h > maxBound.Height)
                {
                    h = maxBound.Height;
                    w = Convert.ToSingle(Math.Round(h * proportion));
                }
            }
            else
                return styleBound;
            // ReSharper restore CompareOfFloatsByEqualityOperator

            return CreateBound(w, h, styleBound.ContentWidth, styleBound.Height);
        }

        public IStyleHelper CreateHelper(IDictionary<Type, IStyle> styles, IStyleSheet styleSheet, IStyledObject subject)
        {
            return new StyleHelper(styles, styleSheet, subject);
        }
    }
}
