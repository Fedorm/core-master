using System;
using MonoTouch.UIKit;
using BitMobile.UI;
using BitMobile.IOS;
using BitMobile.Controls.StyleSheet;

namespace BitMobile.Controls
{
	public class Indicator : Control<UIActivityIndicatorView>, IImageContainer
	{
		int _requests;

		public Indicator ()
		{
            _visible = false;
		}

		public void Start ()
		{
			if (_requests == 0) {
				UIApplication.SharedApplication.BeginIgnoringInteractionEvents ();
				_view.StartAnimating ();
			}
			_requests++;
		}

		public void Stop ()
		{
			if (_requests == 1) {
				_view.StopAnimating ();
				UIApplication.SharedApplication.EndIgnoringInteractionEvents ();
			}
			_requests--;
		}


		public override UIView CreateView ()
		{
			_view = new UIActivityIndicatorView ();
			_view.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray;
			_view.StopAnimating ();
			_view.HidesWhenStopped = true;

			return _view;
		}

		public override Bound Apply (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			base.Apply (stylesheet, styleBound, maxBound);

			StyleHelper style = stylesheet.GetHelper<StyleHelper>();

			// color
			_view.Color = style.ColorOrClear<Color> (this);

			_view.HidesWhenStopped = true;

			return styleBound;
		}

		#region IImageContainer implementation

		public bool InitializeImage (BitMobile.Controls.StyleSheet.StyleSheet stylesheet)
		{
			ImageWidth = 30;
			ImageHeight = 30;

			return true;
		}

		public int ImageWidth {	get; set; }

		public int ImageHeight { get; set; }

		#endregion
	}
}

