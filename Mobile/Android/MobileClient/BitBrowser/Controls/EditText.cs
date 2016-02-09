using Android.Text;
using Android.Views;
using BitMobile.Droid;

namespace BitMobile.Controls
{
    // ReSharper disable once ClassNeverInstantiated.Global, UnusedMember.Global
    public class EditText : CustomEdit
    {
        public EditText(BaseScreen activity)
            : base(activity)
        {
        }

        public override View CreateView()
        {
            base.CreateView();

            _view.InputType |= InputTypes.Null;
            _view.SetSingleLine();
            _view.Ellipsize = Android.Text.TextUtils.TruncateAt.End;            

            return _view;
        }
    }
}
