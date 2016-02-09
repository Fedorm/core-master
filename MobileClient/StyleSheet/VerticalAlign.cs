using System;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    [Synonym("vertical-align")]
    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    public class VerticalAlign : Style<VerticalAlign>, IVerticalAlign
    {
        public VerticalAlign(long depth)
            : base(depth)
        {
        }

        public VerticalAlignValues Align { get; private set; }

        public override void FromString(string s)
        {
            VerticalAlignValues result;
            if (!Enum.TryParse(s, true, out result))
                throw new Exception("Invalid vertical-align value");

            Align = result;
        }

        protected override bool Equals(VerticalAlign other)
        {
            return other.Align == Align;
        }

        protected override int GenerateHashCode()
        {
            return (int) Align;
        }
    }
}