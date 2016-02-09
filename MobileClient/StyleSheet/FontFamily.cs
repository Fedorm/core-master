using System;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    [Synonym("font-family")]
    class FontFamily : FontBase<FontFamily>, IFontFamily
    {
        public FontFamily(long depth)
            : base(depth)
        {
            Family = DefaultFontFamily;
        }

        public string Family { get; private set; }

        public override void FromString(string s)
        {
            Family = ParseFamily(s);
        }

        protected override bool Equals(FontFamily other)
        {
            return string.Equals(other.Family, Family, StringComparison.InvariantCultureIgnoreCase);
        }

        protected override int GenerateHashCode()
        {
            return Family != null ? Family.GetHashCode() : 0;
        }
    }
}
