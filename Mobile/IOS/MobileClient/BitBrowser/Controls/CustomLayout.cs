using System;
using System.Collections.Generic;
using System.Text;
using BitMobile.Controls.StyleSheet;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using BitMobile.IOS;
using BitMobile.UI;
using System.IO;

namespace BitMobile.Controls
{
	public abstract class CustomLayout : Control<CustomLayout.NativeView>, IContainer, IApplicationContextAware, IImageContainer, IValidatable
	{
		protected internal ApplicationContext _applicationContext;
		protected List<Control> _controls = new List<Control> ();
		UIImage _backgroundImageCache = null;
		UIColor _backgroundColor;
		UIColor _selectedColor;
		string _onEvent = "null";
		bool _disposed = false;

		public CustomLayout ()
		{
		}

		public ActionHandler OnClickAction { get; set; }

		public ActionHandlerEx OnClick { get; set; }

		public String OnEvent {
			get {
				return _onEvent;
			}
			set {
				_onEvent = value;
				_applicationContext.SubscribeEvent (value, InvokeClickAction);
			}
		}

		public override UIView CreateView ()
		{
			_view = new NativeView ();
			_view.TouchesBeganEvent += HandleTouchesBeganEvent;
			_view.TouchesEndedEvent += HandleTouchesEndedEvent;
			_view.TouchesCancelledEvent += HandleTouchesCancelledEvent;

			foreach (var control in _controls)
				_view.AddSubview (control.CreateView ());						

			return _view;
		}

		public override Bound Apply (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			base.Apply (stylesheet, styleBound, maxBound);

			StyleHelper style = stylesheet.GetHelper<StyleHelper> ();

			// background color, background image, borders
			if (!InitBackgroundImage (stylesheet))
				style.SetBackgroundSettings (this);
			_backgroundColor = _view.BackgroundColor;

			// selected-color
			_selectedColor = style.Color<SelectedColor> (this);

			return LayoutChildren (stylesheet, styleBound, maxBound);			 
		}

		public override void AnimateTouch (TouchEventType touch)
		{
			if (_selectedColor != null)
				switch (touch) {
				case TouchEventType.Begin:
					_view.BackgroundColor = _selectedColor;
					break;
				case TouchEventType.Cancel:
				case TouchEventType.End:
					UIView.BeginAnimations (null);
					UIView.SetAnimationDuration (0.1);
					_view.BackgroundColor = _backgroundColor;
					UIView.CommitAnimations ();
					break;
				}

			foreach (var control in _controls)
				control.AnimateTouch (touch);
		}

		#region IContainer implementation

		public virtual void AddChild (object obj)
		{
			Control control = obj as Control;
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
			if (InitBackgroundImage (stylesheet)) {
				this.ImageWidth = (int)_backgroundImageCache.Size.Width;
				this.ImageHeight = (int)_backgroundImageCache.Size.Height;
				return true;			
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

		protected abstract Bound LayoutChildren (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound);

		protected override void Dispose (bool disposing)
		{
			if (!_disposed) {
				if (disposing) {
					_applicationContext = null;
				}

				foreach (var control in _controls)
					control.Dispose ();				
				_controls.Clear ();
				_controls = null;

				if (_backgroundColor != null)
					_backgroundColor.Dispose ();
				_backgroundColor = null;

				if (_selectedColor != null)
					_selectedColor.Dispose ();
				_selectedColor = null;

				if (_backgroundImageCache != null)
					_backgroundImageCache.Dispose ();
				_backgroundImageCache = null;

				if (_view != null) {
					_view.TouchesBeganEvent -= HandleTouchesBeganEvent;
					_view.TouchesEndedEvent -= HandleTouchesEndedEvent;
					_view.TouchesCancelledEvent -= HandleTouchesCancelledEvent;
				}

				_disposed = true;
			}

			base.Dispose (disposing);
		}

		protected override void OnSetFrame ()
		{
			if (_backgroundImageCache != null)
				DrawBackgroundImage ();
		}

		void HandleTouchesBeganEvent (NSSet touches, UIEvent evt)
		{
			CloseModalWindows ();
			if (OnClickAction != null || OnClick != null) {
				EndEditing ();

				AnimateTouch (TouchEventType.Begin);				
			} else if (_view.Superview != null)
				_view.Superview.TouchesBegan (touches, evt);
		}

		void HandleTouchesEndedEvent (NSSet touches, UIEvent evt)
		{
			_view.EndEditing (true);
			if (OnClick != null || OnClickAction != null) {
				InvokeClickAction ();		

				AnimateTouch (TouchEventType.End);
			}
		}

		void HandleTouchesCancelledEvent (NSSet touches, UIEvent evt)
		{
			_view.EndEditing (true);

			AnimateTouch (TouchEventType.Cancel);

			if (_view.Superview != null)
				_view.Superview.TouchesCancelled (touches, evt);
		}

		bool InvokeClickAction ()
		{
			if (OnClick != null) {
				OnClick.Execute ();
				return true;
			}

			if (OnClickAction != null) {
				OnClickAction.Execute ();
				return true;
			}
			return false;
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

		public class NativeView : UIView
		{
			public delegate void TouchesDelegate (NSSet touches, UIEvent evt);

			public event TouchesDelegate TouchesBeganEvent;

			public override void TouchesBegan (NSSet touches, UIEvent evt)
			{
				base.TouchesBegan (touches, evt);
				if (TouchesBeganEvent != null)
					TouchesBeganEvent (touches, evt);
			}

			public event TouchesDelegate TouchesEndedEvent;

			public override void TouchesEnded (NSSet touches, UIEvent evt)
			{
				base.TouchesEnded (touches, evt);
				if (TouchesEndedEvent != null)
					TouchesEndedEvent (touches, evt);
			}

			public event TouchesDelegate TouchesCancelledEvent;

			public override void TouchesCancelled (NSSet touches, UIEvent evt)
			{
				base.TouchesCancelled (touches, evt);
				if (TouchesCancelledEvent != null)
					TouchesCancelledEvent (touches, evt);
			}
		}
	}
}
