using System;
using MonoTouch.UIKit;
using System.Drawing;
using BitMobile.Controls.StyleSheet;

namespace BitMobile.Controls
{
	[Synonym ("shl")]
	public class SwipeHorizontalLayout: CustomSwipeLayout
	{
		public SwipeHorizontalLayout ()
		{
		}

		protected override Bound LayoutChildren (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			_behaviour.ScrolledMeasure = styleBound.Width;

			Bound bound = LayoutBehaviour.Horizontal (stylesheet, this, _controls, styleBound, maxBound, true);

			if (_controls.Count > 0) {
				_behaviour.Borders.Add (_controls [0].Frame.Left);
				foreach (var control in _controls) 
					_behaviour.Borders.Add(control.Frame.Right);
			}

			return bound;
		}

		protected override void OnScrollEnded (float startX, float startY)
		{
			float offset = _behaviour.HandleSwipe (_view.ContentOffset.X, startX, (int)_view.ContentOffset.X);
			Scroll (offset);
		}

		protected override void Scroll (float offset)
		{
			if (_view != null) 
				_view.SetContentOffset (new PointF (offset, 0), true);
		}

		public override Bound Apply (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			base.Apply (stylesheet, styleBound, maxBound);

			float offset = _behaviour.OffsetByIndex;
			_view.ContentOffset = new PointF (offset, 0);
			return styleBound;
		}
	}
}

