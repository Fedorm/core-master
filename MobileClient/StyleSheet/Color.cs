using System;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public abstract class Color : Style<Color>, IColor
    {
        protected Color(long depth)
            : base(depth)
        {
            Value = null;
        }

        public IColorInfo Value { get; private set; }

        public override void FromString(string s)
        {
            Value = FromHexString(s);
        }

        protected override bool Equals(Color other)
        {
            return Equals(other.Value, Value);
        }

        protected override int GenerateHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

        static IColorInfo FromHexString(string hexValue)
        {
            if (hexValue.ToLower().Equals("none"))
                return null;

            var colorString = hexValue.Replace("#", "");

            int red, green, blue;

            switch (colorString.Length)
            {
                case 3: // #RGB
                    {
                        red = Convert.ToInt32(string.Format("{0}{0}", colorString.Substring(0, 1)), 16);
                        green = Convert.ToInt32(string.Format("{0}{0}", colorString.Substring(1, 1)), 16);
                        blue = Convert.ToInt32(string.Format("{0}{0}", colorString.Substring(2, 1)), 16);
                        return new ColorInfo(red, green, blue, hexValue);
                    }
                case 6: // #RRGGBB
                    {
                        red = Convert.ToInt32(colorString.Substring(0, 2), 16);
                        green = Convert.ToInt32(colorString.Substring(2, 2), 16);
                        blue = Convert.ToInt32(colorString.Substring(4, 2), 16);
                        return new ColorInfo(red, green, blue, hexValue);
                    }

                default:
                    throw new ArgumentOutOfRangeException(string.Format("Invalid color value {0} is invalid. It should be a hex value of the form #RBG, #RRGGBB", hexValue));
            }
        }
    }

    [Synonym("color")]
    public class TextColor : Color, ITextColor
    {
        public TextColor(long depth)
            : base(depth)
        {
        }
    }

    [Synonym("placeholder-color")]
    public class PlaceholderColor : Color, IPlaceholderColor
    {
        public PlaceholderColor(long depth)
            : base(depth)
        {
        }
    }

    [Synonym("background-color")]
    public class BackgroundColor : Color, IBackgroundColor
    {
        public BackgroundColor(long depth)
            : base(depth)
        {
        }
    }

    [Synonym("selected-color")]
    public class SelectedColor : Color, ISelectedColor
    {
        public SelectedColor(long depth)
            : base(depth)
        {
        }
    }

    [Synonym("selected-background")]
    public class SelectedBackground : Color, ISelectedBackground
    {
        public SelectedBackground(long depth)
            : base(depth)
        {
        }
    }

    [Synonym("border-color")]
    public class BorderColor : Color, IBorderColor
    {
        public BorderColor(long depth)
            : base(depth)
        {
        }
    }
}

