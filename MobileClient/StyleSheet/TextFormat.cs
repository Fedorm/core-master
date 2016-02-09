using System;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    [Synonym("text-format")]
    public class TextFormat : Style<TextFormat>, ITextFormat
    {
        public TextFormat(long depth)
            : base(depth)
        {
        }

        public TextFormatValues Format { get; private set; }

        public override void FromString(string s)
        {
            s = s.Trim();

            TextFormatValues result;
            if (!Enum.TryParse(s, true, out result))
                throw new Exception("Invalid text-format value: " + s);

            Format = result;
        }

        protected override bool Equals(TextFormat other)
        {
            return other.Format == Format;
        }

        protected override int GenerateHashCode()
        {
            return (int)Format;
        }
    }
}
