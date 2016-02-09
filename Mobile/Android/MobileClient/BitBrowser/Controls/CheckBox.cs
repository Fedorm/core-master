using Android.Views;
using Android.Widget;
using BitMobile.Controls.StyleSheet;
using BitMobile.Droid;
using BitMobile.Droid.UI;

namespace BitMobile.Controls
{
    // ReSharper disable MemberCanBeProtected.Global, MemberCanBePrivate.Global, UnusedMember.Global
    public class CheckBox : Control<Android.Widget.CheckBox>, IDataBind
    {
        bool _checked;

        public CheckBox(BaseScreen activity)
            : base(activity)
        {
        }

        public bool Checked
        {
            get
            {
                if (_view != null)
                    return _view.Checked;
                return _checked;
            }
            set
            {
                if (_view != null)
                    _view.Checked = value;
                else
                    _checked = value;
            }
        }

        public override View CreateView()
        {
            _view = new Android.Widget.CheckBox(_activity) { Checked = _checked };
            _view.CheckedChange += CheckBox_CheckedChange;

            return _view;
        }

        // ReSharper disable once RedundantOverridenMember
        public override Bound Apply(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
            return base.Apply(stylesheet, styleBound, maxBound);

            // nope
        }

        #region IDataBind

        [DataBindAttribute("Checked")]
        public DataBinder Value { get; set; }

        public void DataBind()
        {
        }
        #endregion

        void CheckBox_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (Value != null)
                Value.ControlChanged(e.IsChecked);
        }
    }
}