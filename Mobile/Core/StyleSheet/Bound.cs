namespace BitMobile.Controls.StyleSheet
{
    public struct Bound
    {
        public Bound(float width, float height, float contentWidth, float contentHeight)
            : this()
        {
            Width = width;
            Height = height;
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
    }
}