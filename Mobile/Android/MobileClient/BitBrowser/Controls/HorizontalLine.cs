using Android.Views;
using BitMobile.Controls.StyleSheet;
using BitMobile.Droid;
using BitMobile.Droid.UI;

namespace BitMobile.Controls
{
    [Synonym("line")]
    // ReSharper disable once UnusedMember.Global
    class HorizontalLine : Control<View>
    {
        public HorizontalLine(BaseScreen activity)
            : base(activity)
        {
        }

        public override View CreateView()
        {
            return _view = new View(_activity);
        }

        public override Bound Apply(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            // background color
            _view.SetBackgroundColor(stylesheet
                .GetHelper<StyleHelper>()
                .ColorOrTransparent<BackgroundColor>(this));

            return styleBound;
        }
    }
}