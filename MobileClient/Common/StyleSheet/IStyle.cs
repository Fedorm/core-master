namespace BitMobile.Common.StyleSheet
{
    public interface IStyle
    {
        long Depth { get; }
        void FromString(string s);
    }

}
