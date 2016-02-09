using System;
using System.Collections.Generic;
using System.Text;
using MonoTouch.UIKit;
using BitMobile.Controls.StyleSheet;
using BitMobile.UI;
using BitMobile.IOS;

namespace BitMobile.Controls
{
	[Synonym ("body")]
	public class Screen : Control<UIView>, IContainer, IScreen, ICustomStyleSheet, IValidatable
	{
		IControl<UIView> _child;
		bool _disposed = false;

		public Screen ()
		{
		}

		public ActionHandlerEx OnLoading {
			set {
				if (value != null)
					value.Execute ();
			}
		}

		public ActionHandlerEx OnLoad { get; set; }

		public override UIView CreateView ()
		{
			_view = new UIView ();
			_view.AddSubview (_child.CreateView ());

			return _view;
		}

		public override Bound Apply (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			base.Apply (stylesheet, styleBound, maxBound);

			var app = UIScreen.MainScreen.ApplicationFrame;
			Bound bound = new Bound (app.Width, app.Height);

			LayoutBehaviour.Screen (stylesheet, this, _child, bound);

			Rectangle old = _child.Frame;
			_child.Frame = new Rectangle (old.Left + app.Left, old.Top + app.Top, old.Width, old.Height);

			// background color
			_view.BackgroundColor = stylesheet.GetHelper<StyleHelper> ().ColorOrClear<BackgroundColor> (this);

			if (OnLoad != null)
				OnLoad.Execute ();

			Frame = new Rectangle (app.Left, app.Top, bound);

			return bound;
		}

		#region IContainer implementation

		public void AddChild (object obj)
		{
			if (_child == null) {
				IControl<UIView> control = obj as IControl<UIView>;
				if (control == null)
					throw new ArgumentException (string.Format ("Incorrect child: {0}", obj));
				
				_child = control;
				control.Parent = this;
			} else
				throw new Exception ("Only one child is allowed in Screen container");
		}

		public object[] Controls {
			get {		
				return new object[]{ _child };
			}
		}

		public object GetControl (int index)
		{
			return _child;
		}

		#endregion

		#region IScreen implementation

		public void ExitEditMode ()
		{
			_view.EndEditing (true);
		}

		#endregion

		#region ICustomStyleSheet implementation

		public string StyleSheet { get; set; }

		#endregion

		#region IValidatable implementation

		public bool Validate ()
		{
			bool result = true;

			IValidatable validatable = _child as IValidatable;
			if (validatable != null)
				result &= validatable.Validate ();				


			return result;
		}

		#endregion

		protected override void Dispose (bool disposing)
		{
			if (!_disposed) {
				if (_child != null) {
					_child.Dispose ();
					_child = null;
				}

				_disposed = true;
			}

			base.Dispose (disposing);
		}
	}
}