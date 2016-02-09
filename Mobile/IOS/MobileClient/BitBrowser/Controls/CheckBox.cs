using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using BitMobile.Controls.StyleSheet;
using MonoTouch.UIKit;
using BitMobile.IOS;
using BitMobile.UI;

namespace BitMobile.Controls
{
	public class CheckBox : Control<UISwitch>, IDataBind
	{
		bool _checked;
		bool _disposed = false;

		public CheckBox ()
		{
		}

		public Boolean Checked {
			get {
				if (_view != null)
					return _view.On;				
				return _checked;
			}
			set {
				if (_view != null)
					_view.On = value;
				_checked = value;
			}
		}

		public override UIView CreateView ()
		{
			_view = new UISwitch (new RectangleF (0, 0, 20, 20));
			_view.On = _checked;
			_view.ValueChanged += CheckBox_CheckedChange;

			return _view;
		}

		public override Bound Apply (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			return base.Apply (stylesheet, styleBound, maxBound);

			// nope
		}

		#region IDataBind implementation

		[DataBindAttribute ("Checked")]
		public DataBinder Value { get; set; }

		public void DataBind ()
		{
		}

		#endregion

		protected override void Dispose (bool disposing)
		{
			if (!_disposed) {
				if (_view != null)
					_view.ValueChanged -= CheckBox_CheckedChange;
				_disposed = true;
			}

			base.Dispose (disposing);
		}

		void CheckBox_CheckedChange (object sender, EventArgs e)
		{
			CloseModalWindows ();
			EndEditing ();

			if (this.Value != null && !ApplicationContext.Busy)
				this.Value.ControlChanged (_view.On);
		}
	}
}