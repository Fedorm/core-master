using System;
using System.Globalization;

namespace BitMobile.Controls.StyleSheet
{
    public class Size : Style
    {
        public float Value { get; private set; }

        public Measure Measure { get; private set; }

        public bool SizeToContent { get; private set; }

        public override Style FromString(string s)
        {
            s = s.Trim().ToLower();
            string[] split = s.Split(' ');
            try
            {
                string value = split[0];
                if (s == "auto")
                {
                    Value = 100;
                    Measure = Controls.StyleSheet.Measure.Percent;
                }
                else if (value.Contains("px"))
                {
                    String v = value.Replace("px", "");
                    v = v.Replace(',', '.');
                    Value = float.Parse(v, CultureInfo.InvariantCulture);
                    Measure = BitMobile.Controls.StyleSheet.Measure.Pixels;
                }
                else if (value.Contains("%"))
                {
                    String v = value.Replace("%", "");
                    v = v.Replace(',', '.');
                    Value = float.Parse(v, CultureInfo.InvariantCulture);
                    Measure = BitMobile.Controls.StyleSheet.Measure.Percent;
                }
                else if (value.Contains("sp"))
                {
                    String v = value.Replace("sp", "");
                    v = v.Replace(',', '.');
                    Value = float.Parse(v, CultureInfo.InvariantCulture);
                    Measure = Measure.ScreenPercent;
                }
                else if (value.Contains("mm"))
                {
                    String v = value.Replace("mm", "");
                    v = v.Replace(',', '.');
                    Value = float.Parse(v, CultureInfo.InvariantCulture);
                    Measure = Measure.Millimetre;
                }
                else
                    throw new Exception("Unknown measure");

                SizeToContent = split.Length == 2 && split[1].Trim() == "auto";

                return this;
            }
            catch (Exception e)
            {
                throw new Exception("Invalid style value: " + s, e);
            }
        }
    }

    public class Width : Size
    {
    }

    public class Height : Size
    {
    }
}

