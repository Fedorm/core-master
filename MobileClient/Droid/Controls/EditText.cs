using Android.Text;
using BitMobile.Common.Controls;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "EditText")]
    // ReSharper disable once ClassNeverInstantiated.Global, UnusedMember.Global
    public class EditText : CustomEdit
    {
        public EditText(BaseScreen activity)
            : base(activity)
        {
        }

        public override void CreateView()
        {
            base.CreateView();
            
            _view.SetSingleLine();
            _view.Ellipsize = TextUtils.TruncateAt.End;
        }

        protected override bool IsMultiline()
        {
            return false;
        }
    }
}
