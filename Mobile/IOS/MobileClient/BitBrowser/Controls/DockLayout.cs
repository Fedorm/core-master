using System;
using MonoTouch.UIKit;
using BitMobile.Controls.StyleSheet;
using BitMobile.IOS;

namespace BitMobile.Controls
{
	[Synonym ("dl")]
	public class DockLayout : CustomLayout
	{
		public DockLayout ()
		{

		}

		protected override Bound LayoutChildren (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			return LayoutBehaviour.Dock (stylesheet, this, _controls, styleBound, maxBound);
		}
	}
}

