using System;
using System.Globalization;
using BitMobile.Application;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public abstract class Size : Style<Size>, ISize
    {
        public const float DefaultAmount = 0;
        public const Measure DefaultMeasure = Measure.Pixels;

        protected Size(long depth)
            : base(depth)
        {
            Amount = DefaultAmount;
            Measure = DefaultMeasure;
        }

        public float Amount { get; private set; }

        public Measure Measure { get; private set; }

        public bool SizeToContent { get; private set; }

        public abstract float DisplayMetric { get; }

        public float CalcSize(float parentSize, float displayMetric)
        {
            return ConvertSize(Measure, Amount, parentSize, displayMetric);
        }

        public override void FromString(string s)
        {
            s = s.Trim().ToLower();
            string[] split = s.Split(' ');
            try
            {
                string value = split[0];
                if (s == "auto")
                {
                    Amount = 100;
                    Measure = Measure.Percent;
                }
                else if (value.Contains("px"))
                {
                    String v = value.Replace("px", "");
                    v = v.Replace(',', '.');
                    Amount = float.Parse(v, CultureInfo.InvariantCulture);
                    Measure = Measure.Pixels;
                }
                else if (value.Contains("%"))
                {
                    String v = value.Replace("%", "");
                    v = v.Replace(',', '.');
                    Amount = float.Parse(v, CultureInfo.InvariantCulture);
                    Measure = Measure.Percent;
                }
                else if (value.Contains("sp"))
                {
                    String v = value.Replace("sp", "");
                    v = v.Replace(',', '.');
                    Amount = float.Parse(v, CultureInfo.InvariantCulture);
                    Measure = Measure.ScreenPercent;
                }
                else if (value.Contains("mm"))
                {
                    String v = value.Replace("mm", "");
                    v = v.Replace(',', '.');
                    Amount = float.Parse(v, CultureInfo.InvariantCulture);
                    Measure = Measure.Millimetre;
                }
                else if (value.Contains("dp") || value.Contains("dip"))
                {
                    String v = value.Replace("dp", "").Replace("dip", "");
                    v = v.Replace(',', '.');
                    Amount = float.Parse(v, CultureInfo.InvariantCulture);
                    Measure = Measure.Dip;
                }
                else
                    throw new Exception("Unknown measure");

                SizeToContent = split.Length == 2 && split[1].Trim() == "auto";
            }
            catch (Exception e)
            {
                throw new Exception("Invalid style value: " + s, e);
            }


        }

        protected override bool Equals(Size other)
        {
            return Amount.Equals(other.Amount) && Measure == other.Measure && SizeToContent == other.SizeToContent;
        }

        protected override int GenerateHashCode()
        {
            unchecked
            {
                int hashCode = Amount.GetHashCode();
                hashCode = (hashCode * 397) ^ SizeToContent.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Measure;
                return hashCode;
            }
        }
    }

    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Width : Size, IWidth
    {
        public Width(long depth)
            : base(depth)
        {
        }

        public override float DisplayMetric
        {
            get { return ApplicationContext.Current.DisplayProvider.Width; }
        }
    }

    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Height : Size, IHeight
    {
        public Height(long depth)
            : base(depth)
        {
        }

        public override float DisplayMetric
        {
            get { return ApplicationContext.Current.DisplayProvider.Height; }
        }
    }
}

