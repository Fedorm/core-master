using System;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    [Synonym("horizontal-align")]
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class HorizontalAlign : Style<HorizontalAlign>, IHorizontalAlign
    {
        public HorizontalAlign(long depth)
            : base(depth)
        {
        }

        public HorizontalAlignValues Align { get; private set; }

        public override void FromString(string s)
        {
            HorizontalAlignValues result;
            if (!Enum.TryParse(s, true, out result))
                throw new Exception("Invalid horizontal-align value");

            Align = result;
        }

        protected override bool Equals(HorizontalAlign other)
        {
            return other.Align == Align;
        }

        protected override int GenerateHashCode()
        {
            return (int)Align;
        }
    }
}