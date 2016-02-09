using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;
using BitMobile.Controls.StyleSheet;

namespace BitMobile.Controls
{
    public struct Rectangle
    {
        bool _valid;

        public static readonly Rectangle Empty = new Rectangle();

        public Rectangle(float left, float top, float width, float height)
            : this()
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;

            _valid = true;
        }

        public Rectangle(float left, float top, Bound bound)
            : this(left, top, bound.Width, bound.Height)
        {
        }

//        public Rectangle(float left, float top, float width, float height, float maxWidth, float maxHeight)
//            : this()
//        {
//            Left = left;
//            Top = top;
//            Width = width;
//            Height = height;
//            MaxWidth = maxWidth;
//            MaxHeight = maxHeight;
//
//            _valid = true;
//        }
        
        public float Left { get; private set; }

        public float Top { get; private set; }

        public float Right { get { return Left + Width; } }

        public float Bottom { get { return Top + Height; } }

        public float Width { get; private set; }

        public float Height { get; private set; }
//
//        public float MaxWidth { get; set; }
//
//        public float MaxHeight { get; set; }

        public override string ToString()
        {
            return string.Format("Left: {0}; Top: {1}; Width: {2}; Height: {3}"
                , Left, Top, Width, Height);
        }
        public static bool operator ==(Rectangle a, Rectangle b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Rectangle a, Rectangle b)
        {
            return !a.Equals(b);
        }

        public bool Equals(Rectangle obj)
        {
            bool result = true;

            result &= _valid.Equals(obj._valid);
            result &= Left.Equals(obj.Left);
            result &= Top.Equals(obj.Top);
            result &= Width.Equals(obj.Width);
            result &= Height.Equals(obj.Height);

            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj is Rectangle)
                return Equals((Rectangle)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return (int)Left ^ (int)Top ^ (int)Width ^ (int)Height;
        }
    }
}
