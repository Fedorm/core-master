using BitMobile.Application.StyleSheet;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;

namespace BitMobile.Controls
{
    public struct Rectangle : IRectangle
    {
        private readonly bool _valid;

        public Rectangle(float left, float top, IBound bound)
            : this()
        {
            Left = left;
            Top = top;
            Bound = bound;

            _valid = true;
        }

        public Rectangle(float left, float top, float width, float height)
            : this(left, top, StyleSheetContext.Current.CreateBound(width > 0 ? width : 0, height > 0 ? height : 0))
        {
        }

        public float Left { get; private set; }

        public float Top { get; private set; }

        public float Right { get { return Left + Width; } }

        public float Bottom { get { return Top + Height; } }

        public float Width { get { return Bound.Width; } }

        public float Height { get { return Bound.Height; } }

        public bool Empty
        {
            get { return Equals(new Rectangle(0, 0, 0, 0)); }
        }

        public IBound Bound { get; private set; }

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

        public bool Equals(IRectangle obj)
        {
            if (!(obj is Rectangle))
                return false;
            var rect = (Rectangle)obj;

            bool result = true;

            result &= _valid.Equals(rect._valid);
            result &= Left.Equals(rect.Left);
            result &= Top.Equals(rect.Top);
            result &= Width.Equals(rect.Width);
            result &= Height.Equals(rect.Height);

            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj is Rectangle)
                return Equals((Rectangle)obj);
            return false;
        }

        public override int GetHashCode()
        {
            return (int)Left ^ (int)Top ^ (int)Width ^ (int)Height;
        }
    }
}
