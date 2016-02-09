using System;
using System.Collections.Generic;
using BitMobile.Controls.StyleSheet;
using MonoTouch.UIKit;
using BitMobile.IOS;
using BitMobile.UI;
using System.Drawing;
using System.IO;

namespace BitMobile.Controls
{
	public abstract class CustomSwipeLayout: Control<UIScrollView>, IContainer,IApplicationContextAware, IImageContainer, IValidatable
	{
		protected internal ApplicationContext _applicationContext;
		protected List<IControl<UIView>> _controls = new List<IControl<UIView>> ();
		protected SwipeBehaviour _behaviour;
		protected float _startX;
		protected float _startY;
		protected float _previousX;
		protected float _previousY;
		UIImage _backgroundImageCache = null;
		bool disposed = false;

		public CustomSwipeLayout ()
		{
			_behaviour = new SwipeBehaviour (Scroll);

			this.Scrollable = true;
		}

		public int Index {
			get { return _behaviour.Index; }
			set { _behaviour.Index = value; }
		}

		public int Percent {
			get { return (int)Math.Round (_behaviour.Percent * 100); }
			set { _behaviour.Percent = (float)value / 100; }
		}

		public string Alignment {
			get { return _behaviour.Alignment; }
			set { _behaviour.Alignment = value; }
		}

		public bool Scrollable { get; set; }

		public ActionHandlerEx OnSwipe { get; set; }

		public override UIView CreateView ()
		{
			_view = new UIScrollView ();
			_view.ShowsVerticalScrollIndicator = false;
			_view.ShowsHorizontalScrollIndicator = false;
			_view.Bounces = false; 
			_view.DecelerationRate = UIScrollView.DecelerationRateFast;
			_view.Delegate = new ScrollViewDelegate (this);

			_view.ScrollEnabled = Scrollable;

			foreach (var control in _controls) {
				_view.AddSubview (control.CreateView ());
			}	

			return _view;
		}

		public override Bound Apply (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			base.Apply (stylesheet, styleBound, maxBound);

			StyleHelper style = stylesheet.GetHelper<StyleHelper> ();

			// background color, background image, borders
			if (!InitBackgroundImage (stylesheet))
				style.SetBackgroundSettings (this);

			Bound bound = LayoutChildren (stylesheet, styleBound, maxBound);

			_view.ContentSize = new SizeF (bound.ContentWidth, bound.ContentHeight);

			return styleBound;
		}

		#region IContainer implementation

		public void AddChild (object obj)
		{
			IControl<UIView> control = obj as IControl<UIView>;
			if (control == null)
				throw new Exception (string.Format ("Incorrect child: {0}", obj));

			_controls.Add (control);
			control.Parent = this;
		}

		public object[] Controls {
			get {
				return _controls.ToArray ();
			}
		}

		public object GetControl (int index)
		{
			return _controls [index];
		}

		#endregion

		#region IApplicationContextAware implementation

		public void SetApplicationContext (object applicationContext)
		{
			_applicationContext = (ApplicationContext)applicationContext;
		}

		#endregion

		#region IImageContainer implementation

		public bool InitializeImage (BitMobile.Controls.StyleSheet.StyleSheet stylesheet)
		{
			String imgPath = stylesheet.GetHelper<StyleHelper> ().BackgroundImage (this);
			if (imgPath != null) {
				System.IO.Stream imgStream = _applicationContext.DAL.GetImageByName (imgPath);
				if (imgStream != null) {
					_backgroundImageCache = UIImage.LoadFromData (MonoTouch.Foundation.NSData.FromStream (imgStream));

					ImageWidth = (int)_backgroundImageCache.Size.Width;
					ImageHeight = (int)_backgroundImageCache.Size.Height;

					return true;
				}
			}
			return false;
		}

		public int ImageWidth {	get; set; }

		public int ImageHeight { get; set; }

		#endregion

		#region IValidatable implementation

		public bool Validate ()
		{
			bool result = true;

			foreach (var control in _controls) {
				IValidatable validatable = control as IValidatable;
				if (validatable != null)
					result &= validatable.Validate ();				
			}

			return result;
		}

		#endregion

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				if (_view != null && _view.Delegate != null) {
					_view.Delegate.Dispose ();
					_view.Delegate = null;
				}				
			}

			disposed = true;

			base.Dispose (disposing);
		}

		protected override void OnSetFrame ()
		{
			if (_backgroundImageCache != null)
				DrawBackgroundImage ();
		}

		protected abstract Bound LayoutChildren (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound);

		protected abstract void OnScrollEnded (float startX, float startY);

		protected abstract void Scroll (float offset);

		void HandleDraggingStarted ()
		{
			CloseModalWindows ();
			EndEditing ();

			_previousX = _startX;
			_previousY = _startY;
			if (_view != null) {
				_startX = _view.ContentOffset.X;
				_startY = _view.ContentOffset.Y;
			}
		}

		void HandleDraggingEnded (bool willDecelerate)
		{
			if (_view != null)
			if (_startX != _view.ContentOffset.X || _startY != _view.ContentOffset.Y) {
				OnScrollEnded (_startX, _startY);	

				if (OnSwipe != null)
					OnSwipe.Execute ();
			}
		}

		void HandleDecelerationEnded ()
		{
			if (_view != null) {
				if (_startX != _view.ContentOffset.X || _startY != _view.ContentOffset.Y)
					OnScrollEnded (_startX, _startY);
				else
					OnScrollEnded (_previousX, _previousY);
			}
		}

		void DrawBackgroundImage ()
		{
			UIImage img = null;
			try {
				UIGraphics.BeginImageContext (_view.Frame.Size);
				_backgroundImageCache.Draw (_view.Bounds);
				img = UIGraphics.GetImageFromCurrentImageContext ();
				if (img != null)
					_view.BackgroundColor = UIColor.FromPatternImage (img);
			} finally {
				UIGraphics.EndImageContext ();
				if (img != null)
					img.Dispose ();
			}
		}

		bool InitBackgroundImage (StyleSheet.StyleSheet stylesheet)
		{
			if (_backgroundImageCache == null) {
				String imgPath = stylesheet.GetHelper<StyleHelper> ().BackgroundImage (this);
				if (imgPath != null) {
					Stream imgStream = _applicationContext.DAL.GetImageByName (imgPath);
					if (imgStream != null)
						_backgroundImageCache = UIImage.LoadFromData (MonoTouch.Foundation.NSData.FromStream (imgStream));					
				}
			} 
			return _backgroundImageCache != null;
		}

		class ScrollViewDelegate: UIScrollViewDelegate
		{
			CustomSwipeLayout _swipeLayout;

			public ScrollViewDelegate (CustomSwipeLayout swipeLayout)
			{
				_swipeLayout = swipeLayout;
			}

			public override void DraggingStarted (UIScrollView scrollView)
			{
				_swipeLayout.HandleDraggingStarted ();
			}

			public override void DraggingEnded (UIScrollView scrollView, bool willDecelerate)
			{
				_swipeLayout.HandleDraggingEnded (willDecelerate);
			}

			public override void DecelerationEnded (UIScrollView scrollView)
			{
				_swipeLayout.HandleDecelerationEnded ();
			}
		}
	}
}

