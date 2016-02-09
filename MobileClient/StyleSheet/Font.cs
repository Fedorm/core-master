using System;
using BitMobile.Application;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    public class Font : FontBase<Font>, IFont
    {
        public Font(long depth)
            : base(depth)
        {
            Family = DefaultFontFamily;
        }

        public float Value { get; private set; }

        public Measure Measure { get; private set; }

        public string Family { get; private set; }

        public float CalcSize(float parentSize)
        {
            return ConvertSize(Measure, Value, parentSize, ApplicationContext.Current.DisplayProvider.Height);
        }

        public override void FromString(string s)
        {
            string[] arr = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (arr.Length < 2)
                throw new Exception("Cannot parse font: " + s);

            Family = ParseFamily(arr[1]);

            float size;
            Measure measure;
            ParseSize(arr[0], out size, out measure);
            Value = size;
            Measure = measure;
        }

        protected override bool Equals(Font other)
        {
            return string.Equals(Family, other.Family) && Value.Equals(other.Value) && Measure == other.Measure;
        }

        protected override int GenerateHashCode()
        {
            unchecked
            {
                int hashCode = Family != null ? Family.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ Value.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Measure;
                return hashCode;
            }
        }
    }
}

