using System;
using BitMobile.Application;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    public abstract class Style<T> : IStyle where T : Style<T>
    {
        protected Style(long depth)
        {
            Depth = depth;
        }

        public long Depth { get; private set; }

        public abstract void FromString(string s);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            // we aren't using Depth in Equals, because that property for comparing different styles
            return Equals((T)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return GenerateHashCode();
            }
        }

        protected abstract bool Equals(T other);

        protected abstract int GenerateHashCode();

        protected static float ConvertSize(Measure measure, float amount, float parentSize, float displayMetric)
        {
            float result;
            switch (measure)
            {
                case Measure.Pixels:
                    result = amount / Application.StyleSheet.StyleSheetContext.Current.Scale;
                    break;
                case Measure.Percent:
                    result = amount * parentSize / 100;
                    break;
                case Measure.ScreenPercent:
                    result = amount * displayMetric / 100;
                    break;
                case Measure.Millimetre:
                    double px = amount * ApplicationContext.Current.DisplayProvider.PxPerMm;
                    result = (int)Math.Round(px);
                    break;
                case Measure.Dip:
                    const float maxWidth = 980;
                    float coeff = ApplicationContext.Current.DisplayProvider.Width / maxWidth;
                    result = amount * coeff;
                    break;
                default:
                    result = amount;
                    break;
            }
            return result;
        }
    }
}

