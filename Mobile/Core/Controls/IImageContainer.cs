namespace BitMobile.Controls
{
    public interface IImageContainer
    {
        bool InitializeImage(StyleSheet.StyleSheet stylesheet);

        int ImageWidth { get; set; }
        int ImageHeight { get; set; }
    }
}