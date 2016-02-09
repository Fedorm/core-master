using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoTouch.UIKit;
using BitMobile.Controls.StyleSheet;
using BitMobile.IOS;
using System.Drawing;

namespace BitMobile.Controls
{
	[Synonym ("hl")]
	public class HorizontalLayout : CustomLayout
	{
		public HorizontalLayout ()
		{
		}

		protected override Bound LayoutChildren (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			return LayoutBehaviour.Horizontal (stylesheet, this, _controls, styleBound, maxBound);
		}
	}
}