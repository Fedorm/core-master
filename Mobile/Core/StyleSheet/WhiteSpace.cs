using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BitMobile.Controls.StyleSheet
{
    [Synonym("white-space")]
    public class WhiteSpace : Style
    {
        public Kind Value { get; private set; }

        public override Style FromString(string s)
        {
            s = s.Trim();

            Kind result;
            if (!Enum.TryParse<Kind>(s, true, out result))
                throw new Exception("Invalid white-space value: " + s);

            this.Value = result;
            return this;
        }

        public enum Kind
        {
            Normal,
            Nowrap
        }
    }


}