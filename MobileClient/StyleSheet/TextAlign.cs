using System;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    [Synonym("text-align")]
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    public class TextAlign : Style<TextAlign>, ITextAlign
    {
        public TextAlign(long depth)
            : base(depth)
        {
        }

        public TextAlignValues Align { get; private set; }

        public override void FromString(string s)
        {
            s = s.Trim();

            TextAlignValues result;
            if (!Enum.TryParse(s, true, out result))
                throw new Exception("Invalid text-align value: " + s);

            Align = result;
        }

        protected override bool Equals(TextAlign other)
        {
            return other.Align == Align;
        }

        protected override int GenerateHashCode()
        {
            return (int)Align;
        }
    }
}

