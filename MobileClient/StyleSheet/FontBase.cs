using System;
using System.Globalization;

namespace BitMobile.StyleSheet
{
    public abstract class FontBase<T> : Style<T> where T : FontBase<T>
    {
        public const string DefaultFontFamily = "arial";

        protected FontBase(long depth)
            : base(depth)
        {
        }

        protected string ParseFamily(string input)
        {
            return input.Trim();
        }

        protected void ParseSize(string input, out float size, out Measure measure)
        {
            try
            {
                if (input.Contains("px"))
                {
                    string v = input.Replace("px", "");
                    v = v.Replace(',', '.');
                    size = float.Parse(v, CultureInfo.InvariantCulture);
                    measure = Measure.Pixels;
                }
                else if (input.Contains("%"))
                {
                    string v = input.Replace("%", "");
                    v = v.Replace(',', '.');
                    size = float.Parse(v, CultureInfo.InvariantCulture);
                    measure = Measure.Percent;
                }
                else if (input.Contains("sp"))
                {
                    string v = input.Replace("sp", "");
                    v = v.Replace(',', '.');
                    size = float.Parse(v, CultureInfo.InvariantCulture);
                    measure = Measure.ScreenPercent;
                }
                else if (input.Contains("mm"))
                {
                    string v = input.Replace("mm", "");
                    v = v.Replace(',', '.');
                    size = float.Parse(v, CultureInfo.InvariantCulture);
                    measure = Measure.Millimetre;
                }
                else if (input.Contains("dp"))
                {
                    string v = input.Replace("dp", "");
                    v = v.Replace(',', '.');
                    size = float.Parse(v, CultureInfo.InvariantCulture);
                    measure = Measure.Dip;
                }
                else
                    throw new Exception("Unknown measure");
            }
            catch (Exception e)
            {
                throw new Exception("Cannot parse font-size: " + input, e);
            }
        }
    }
}