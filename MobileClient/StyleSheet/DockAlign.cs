using System;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    [Synonym("dock-align")]
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class DockAlign : Style<DockAlign>, IDockAlign
    {
        public DockAlign(long depth)
            : base(depth)
        {
            Align = DockAlignValues.Top;
        }

        public DockAlignValues Align { get; private set; }
        
        public override void FromString(string s)
        {
            DockAlignValues result;
            if (!Enum.TryParse(s, true, out result))
                throw new Exception("Invalid dock-align value");

            Align = result;
        }

        protected override bool Equals(DockAlign other)
        {
            return other.Align == Align;
        }

        protected override int GenerateHashCode()
        {
            return (int) Align;
        }
    }
}