using System;
using BitMobile.Controls;
using MonoTouch.UIKit;
using BitMobile.Controls.StyleSheet;
using BitMobile.Utilities.LogManager;
using BitMobile.IOS;

namespace BitMobile.UI
{
	public abstract class Control: IControl<UIView>
	{
		bool _disposed = false;

		public Control ()
		{
		}

		public string Id { get; set; }

		public abstract bool Visible { get; set; }

		public virtual void AnimateTouch (TouchEventType touch)
		{
		}

		#region IControl implementation

		public abstract UIView CreateView ();

		[NonLog]
		public abstract UIView View { get; }

		[NonLog]
		public object Parent { get; set; }

		#endregion

		#region ILayoutable implementation

		public abstract Rectangle Frame { get; set; }

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		#endregion

		#region IStyledObject implementation

		public abstract Bound Apply (StyleSheet stylesheet, Bound styleBound, Bound maxBound);

		public string CssClass { get; set; }

		#endregion

		protected virtual void Dispose (bool disposing)
		{
			if (!_disposed) {

				_disposed = true;
			}
		}

		~Control ()
		{
			Dispose (false);
		}

		public enum TouchEventType
		{
			Begin,
			End,
			Cancel
		}
	}
}

