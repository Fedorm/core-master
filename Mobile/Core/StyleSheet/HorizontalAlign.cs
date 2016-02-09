using System;

namespace BitMobile.Controls.StyleSheet
{
    [Synonym("horizontal-align")]
    public class HorizontalAlign : Style
    {
        public Align Value { get; private set; }

        public override Style FromString(string s)
        {
            Align result;
            if (!Enum.TryParse<Align>(s, true, out result))
                throw new Exception("Invalid horizontal-align value");

            this.Value = result;
            return this;
        }

        public enum Align
        {
            Left,
            Central,
            Right
        }
    }
}