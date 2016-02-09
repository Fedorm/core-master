namespace BitMobile.Common.StyleSheet
{
    public interface IStyleKeyPart
    {
        uint Type { get; }

        uint CssClass { get; }
    }
}