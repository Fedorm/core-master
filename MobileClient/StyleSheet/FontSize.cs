using BitMobile.Application;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    [Synonym("font-size")]
    class FontSize : FontBase<FontSize>, IFontSize
    {
        public FontSize(long depth)
            : base(depth)
        {
        }

        public float Size { get; private set; }

        public Measure Measure { get; private set; }
        
        public float CalcSize(float parentSize)
        {
            return ConvertSize(Measure, Size, parentSize, ApplicationContext.Current.DisplayProvider.Height);
        }

        public override void FromString(string s)
        {
            float size;
            Measure measure;
            ParseSize(s, out size, out measure);
            Size = size;
            Measure = measure;
        }

        protected override bool Equals(FontSize other)
        {
            return Size.Equals(other.Size) && Measure == other.Measure;
        }

        protected override int GenerateHashCode()
        {
            unchecked
            {
                return (Size.GetHashCode()*397) ^ (int) Measure;
            }
        }
    }
}