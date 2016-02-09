using System;

namespace BitMobile.Controls.StyleSheet
{
    [Synonym("vertical-align")]
    public class VerticalAlign : Style
    {
        public Align Value { get; private set; }

        public override Style FromString(string s)
        {
            Align result;
            if (!Enum.TryParse<Align>(s, true, out result))
                throw new Exception("Invalid vertical-align value");

            this.Value = result;
            return this;
        }

        public enum Align
        {
            Top,
            Central,
            Bottom
        }
    }
}