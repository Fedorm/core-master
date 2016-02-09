using System;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    [Synonym("border-style")]
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class Border : Style<Border>, IBorderStyle
    {

        public Border(long depth)
            : base(depth)
        {
        }

        public BorderStyleValues Style { get; set; }

        public override void FromString(string s)
        {
            s = s.Trim();
            BorderStyleValues result;
            if (!Enum.TryParse(s, true, out result))
                throw new Exception("Invalid border value");
            Style = result;
        }

        protected override bool Equals(Border other)
        {
            return other.Style == Style;
        }

        protected override int GenerateHashCode()
        {
            return Style.GetHashCode();
        }
    }

    [Synonym("border-width")]
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BorderWidth : Size, IBorderWidth
    {
        public BorderWidth(long depth)
            : base(depth)
        {
        }

        public float Value { get; private set; }

        public override float DisplayMetric
        {
            get { return 0; }
        }

        public override void FromString(string s)
        {
            base.FromString(s);
            Value = ConvertSize(Measure, Amount, 0, 0);
        }
    }
}

