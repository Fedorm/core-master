using System.Diagnostics;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    [DebuggerDisplay("{Width}:{Height}/{ContentWidth}:{ContentHeight}")]
    public struct Bound : IBound
    {
        public Bound(float width, float height, float contentWidth, float contentHeight)
            : this()
        {
			Width = width > 0 ? width : 0;
			Height = height > 0 ? height: 0;
            ContentWidth = contentWidth;
            ContentHeight = contentHeight;
        }

        public Bound(float width, float height)
            : this(width, height, width, height)
        {
        }

        public float Width { get; private set; }

        public float Height { get; private set; }

        public float ContentWidth { get; private set; }

        public float ContentHeight { get; private set; }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = ContentHeight.GetHashCode();
                hashCode = (hashCode*397) ^ ContentWidth.GetHashCode();
                hashCode = (hashCode*397) ^ Height.GetHashCode();
                hashCode = (hashCode*397) ^ Width.GetHashCode();
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Bound && Equals((Bound)obj);
        }

        private bool Equals(Bound other)
        {
            return ContentHeight.Equals(other.ContentHeight)
                && ContentWidth.Equals(other.ContentWidth)
                && Height.Equals(other.Height)
                && Width.Equals(other.Width);
        }
    }
}