using BitMobile.Application;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    public abstract class Padding : Size
    {
        protected Padding(long depth)
            : base(depth)
        {
        }
    }

    [Synonym("padding-left")]
    public class PaddingLeft : Padding, IPaddingLeft
    {
        public PaddingLeft(long depth)
            : base(depth)
        {
        }

        public override float DisplayMetric
        {
            get { return ApplicationContext.Current.DisplayProvider.Width; }
        }
    }

    [Synonym("padding-top")]
    public class PaddingTop : Padding, IPaddingTop
    {
        public PaddingTop(long depth)
            : base(depth)
        {
        }

        public override float DisplayMetric
        {
            get { return ApplicationContext.Current.DisplayProvider.Height; }
        }
    }

    [Synonym("padding-right")]
    public class PaddingRight : Padding, IPaddingRight
    {
        public PaddingRight(long depth)
            : base(depth)
        {
        }

        public override float DisplayMetric
        {
            get { return ApplicationContext.Current.DisplayProvider.Width; }
        }
    }

    [Synonym("padding-bottom")]
    public class PaddingBottom : Padding, IPaddingBottom
    {
        public PaddingBottom(long depth)
            : base(depth)
        {
        }

        public override float DisplayMetric
        {
            get { return ApplicationContext.Current.DisplayProvider.Height; }
        }
    }
}

