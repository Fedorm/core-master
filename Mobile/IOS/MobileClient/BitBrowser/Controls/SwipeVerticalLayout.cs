using System;
using MonoTouch.UIKit;
using System.Drawing;
using BitMobile.Controls.StyleSheet;

namespace BitMobile.Controls
{
	[Synonym ("svl")]
	public class SwipeVerticalLayout: CustomSwipeLayout
	{
		public SwipeVerticalLayout ()
		{
		}

		protected override Bound LayoutChildren (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			_behaviour.ScrolledMeasure = styleBound.Height;

			Bound bound = LayoutBehaviour.Vertical (stylesheet, this, _controls, styleBound, maxBound, true);

			if (_controls.Count > 0) {
				_behaviour.Borders.Add (_controls [0].Frame.Top);
				foreach (var control in _controls) 
					_behaviour.Borders.Add(control.Frame.Bottom);
			}

			return bound;
		}

		protected override void OnScrollEnded (float startX, float startY)
		{
			float offset = _behaviour.HandleSwipe (_view.ContentOffset.Y, startY, (int)_view.ContentOffset.Y);
			Scroll (offset);
		}

		protected override void Scroll (float offset)
		{
			if (_view != null) 
				_view.SetContentOffset (new PointF (0, offset), true);				
		}

		public override Bound Apply (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			base.Apply (stylesheet, styleBound, maxBound);

			float offset = _behaviour.OffsetByIndex;
			_view.ContentOffset = new PointF (0, offset);
			return styleBound;
		}
	}
}

