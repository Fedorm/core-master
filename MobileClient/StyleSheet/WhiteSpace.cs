using System;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    [Synonym("white-space")]
    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    public class WhiteSpace : Style<WhiteSpace>, IWhiteSpace
    {
        public WhiteSpace(long depth)
            : base(depth)
        {
        }

        public WhiteSpaceKind Kind { get; private set; }

        public override void FromString(string s)
        {
            s = s.Trim();

            WhiteSpaceKind result;
            if (!Enum.TryParse(s, true, out result))
                throw new Exception("Invalid white-space value: " + s);

            Kind = result;
        }

        protected override bool Equals(WhiteSpace other)
        {
            return other.Kind == Kind;
        }

        protected override int GenerateHashCode()
        {
            return (int)Kind;
        }
    }


}