using Android.Text;
using BitMobile.Common.Controls;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "MemoEdit")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MemoEdit : CustomEdit
    {
        public MemoEdit(BaseScreen activity)
            : base(activity)
        {
        }

        protected override bool IsMultiline()
        {
            return true;
        }
    }
}
