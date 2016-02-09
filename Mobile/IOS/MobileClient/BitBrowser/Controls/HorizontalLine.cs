using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoTouch.UIKit;
using BitMobile.UI;
using BitMobile.Controls.StyleSheet;
using BitMobile.IOS;

namespace BitMobile.Controls
{
	[Synonym ("line")]
	public class HorizontalLine : Control<UIView>
	{
		public HorizontalLine ()
		{
		}

		public override UIView CreateView ()
		{
			_view = new UIView ();

			return _view;
		}

		public override Bound Apply (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			base.Apply (stylesheet, styleBound, maxBound);

			// background color
			_view.BackgroundColor = stylesheet.GetHelper<StyleHelper>().ColorOrClear<BackgroundColor> (this);

			return styleBound;
		}
	}
}