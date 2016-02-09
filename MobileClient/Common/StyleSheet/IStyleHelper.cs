namespace BitMobile.Common.StyleSheet
{
    public interface IStyleHelper
    {
        T Get<T>() where T : IStyle;
        bool TryGet<T>(out T style) where T : IStyle;
        bool Exsists<T>() where T : IStyle;

        bool TryGetFontSize(float parentSize, out float size);
        bool TryGetFontFamily(out string family);
        float GetSizeOrDefault<T>(float parentSize, int defaultValue) where T : ISize;
    }
}