using BitMobile.Common.StyleSheet;

namespace BitMobile.Common.Controls
{
    public interface IRectangle
    {
        float Left { get; }
        float Top { get; }
        float Right { get; }
        float Bottom { get; }
        float Width { get; }
        float Height { get; }
        bool Empty { get; }
        IBound Bound { get; }
        string ToString();
        bool Equals(IRectangle obj);
        bool Equals(object obj);
        int GetHashCode();
    }
}
