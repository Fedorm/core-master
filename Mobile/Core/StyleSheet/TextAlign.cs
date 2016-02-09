using System;

namespace BitMobile.Controls.StyleSheet
{
    [Synonym("text-align")]
    public class TextAlign : Style
    {
        public Align Value { get; private set; }

        public override Style FromString(string s)
        {
            s = s.Trim();

            Align result;
            if (!Enum.TryParse<Align>(s, true, out result))
                throw new Exception("Invalid text-align value: " + s);
            
            this.Value = result;
            return this;
        }

        public enum Align
        {
            Left,
            Center,
            Right
        }
    }


}

