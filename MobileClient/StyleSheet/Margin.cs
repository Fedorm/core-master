using BitMobile.Application;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet
{
    public abstract class Margin : Size
    {
        protected Margin(long depth)
            : base(depth)
        {
        }
    }

    [Synonym("margin-left")]
    public class MarginLeft : Margin, IMarginLeft
    {
        public MarginLeft(long depth)
            : base(depth)
        {
        }

        public override float DisplayMetric
        {
            get { return ApplicationContext.Current.DisplayProvider.Width; }
        }
    }

    [Synonym("margin-top")]
    public class MarginTop : Margin, IMarginTop
    {
        public MarginTop(long depth)
            : base(depth)
        {
        }

        public override float DisplayMetric
        {
            get { return ApplicationContext.Current.DisplayProvider.Height; }
        }
    }

    [Synonym("margin-right")]
    public class MarginRight : Margin, IMarginRight
    {
        public MarginRight(long depth)
            : base(depth)
        {
        }

        public override float DisplayMetric
        {
            get { return ApplicationContext.Current.DisplayProvider.Width; }
        }
    }

    [Synonym("margin-bottom")]
    public class MarginBottom : Margin, IMarginBottom
    {
        public MarginBottom(long depth)
            : base(depth)
        {
        }

        public override float DisplayMetric
        {
            get { return ApplicationContext.Current.DisplayProvider.Height; }
        }
    }
}

