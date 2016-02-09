using System;
using System.Drawing;
using BitMobile.Common.Controls;
using BitMobile.IOS;
using BitMobile.UI;
using MonoTouch.UIKit;
using System.Collections.Generic;
using BitMobile.Common.StyleSheet;

namespace BitMobile.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "CheckBox")]
    public class CheckBox : Control<UISwitch>, IDataBind
    {
        private bool _checked;

        public Boolean Checked
        {
            get
            {
                if (_view != null)
                    return _view.On;
                return _checked;
            }
            set
            {
                if (_view != null)
                    _view.On = value;
                _checked = value;
            }
        }

        public override void CreateView()
        {
            _view = new UISwitch(new RectangleF(0, 0, 20, 20));
            _view.On = _checked;
            _view.ValueChanged += CheckBox_CheckedChange;
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            // nope
            return styleBound;
        }

        private void CheckBox_CheckedChange(object sender, EventArgs e)
        {
            CloseModalWindows();
            EndEditing();

            if (Value != null && !IOSApplicationContext.Busy)
                Value.ControlChanged(_view.On);
        }

        protected override void Dismiss()
        {
            _view.ValueChanged -= CheckBox_CheckedChange;
        }

        #region IDataBind implementation

        [DataBind("Checked")]
        public IDataBinder Value { get; set; }

        public void DataBind()
        {
        }

        #endregion
    }
}