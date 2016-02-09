using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoTouch.UIKit;
using BitMobile.Controls.StyleSheet;
using BitMobile.IOS;

namespace BitMobile.Controls
{
	[Synonym ("vl")]
	public partial class VerticalLayout : CustomLayout
	{
		public VerticalLayout ()
		{
		}

		protected override Bound LayoutChildren (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			return LayoutBehaviour.Vertical (stylesheet, this, _controls, styleBound, maxBound);
		}
	}
}