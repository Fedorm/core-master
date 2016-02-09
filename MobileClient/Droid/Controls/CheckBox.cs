using System;
using System.Collections.Generic;
using Android.Widget;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.Controls;
using BitMobile.Droid.UI;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "CheckBox")]
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

        public override void CreateView()
        {
            _view = new Android.Widget.CheckBox(Activity) { Checked = _checked };
            _view.CheckedChange += CheckBox_CheckedChange;
        }

        #region IDataBind

        [DataBind("Checked")]
        public IDataBinder Value { get; set; }

        public void DataBind()
        {
        }
        #endregion

        // ReSharper disable once RedundantOverridenMember
        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            // nope
            return base.Apply(stylesheet, styleBound, maxBound);
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            // nope
            return styleBound;
        }
        
        void CheckBox_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (Value != null)
                Value.ControlChanged(e.IsChecked);
        }
    }
}