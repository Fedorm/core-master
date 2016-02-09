using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    [Synonym("border-radius")]
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BorderRadius : Size, IBorderRadius
    {
        public BorderRadius(long depth)
            : base(depth)
        {
        }

        public float Radius { get; private set; }

        public override float DisplayMetric
        {
            get { return 0; }
        }

        public override void FromString(string s)
        {
            base.FromString(s);
            Radius = ConvertSize(Measure, Amount, 0, 0);
        }
    }


}

