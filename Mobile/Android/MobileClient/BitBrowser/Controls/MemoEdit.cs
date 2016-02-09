using Android.Text;
using BitMobile.Droid;

namespace BitMobile.Controls
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MemoEdit : CustomEdit
    {
        public MemoEdit(BaseScreen activity)
            : base(activity)
        {
        }

        public override Android.Views.View CreateView()
        {
            base.CreateView();
            _view.InputType |= InputTypes.TextFlagMultiLine;
            return _view;
        }
    }
}
