using System;

namespace BitMobile.Controls.StyleSheet
{
    [Synonym("dock-align")]
    public class DockAlign : Style
    {
        public Align Value { get; private set; }

        public override Style FromString(string s)
        {
            Align result;
            if (!Enum.TryParse<Align>(s, true, out result))
                throw new Exception("Invalid dock-align value");

            this.Value = result;
            return this;
        }

        public enum Align
        {
            Left,
            Top,
            Right,
            Bottom
        }
    }
}