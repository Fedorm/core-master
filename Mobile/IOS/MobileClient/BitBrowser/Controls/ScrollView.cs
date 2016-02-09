using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using BitMobile.Controls.StyleSheet;
using BitMobile.UI;
using System.Collections.Generic;
using BitMobile.IOS;
using System.Drawing;
using BitMobile.Utilities.Develop;
using Common.Controls;
using System.Threading;

namespace BitMobile.Controls
{
	[Synonym ("sv")]
	public class ScrollView : Control<ScrollView.NativeTableView>, IContainer, IValidatable, IApplicationContextAware, IPersistable
	{
		ApplicationContext _applicationContext = null;
		List<IControl<UIView>> _controls = new List<IControl<UIView>> ();
		StyleSheet.StyleSheet _stylesheet;
		int _scrollIndex;
		PointF? _scrollOffset;
		bool _hideOverlaysAfterScrolling = true;
		DateTime _scrollAnimationFinished;
		Action _scrollEndedCallback;
		bool _disposed = false;

		public ScrollView ()
		{			
		}

		public int Index {
			get {
				if (_view != null && _controls.Count > 0) {
					NSIndexPath path = _view.IndexPathsForVisibleRows [0];
					_scrollIndex = path.Row;
				}

				return _scrollIndex;
			}
			set {
				_scrollIndex = value;
				_scrollOffset = null;
				if (_view != null)
					SetIndex ();
			}
		}

		public ActionHandlerEx OnScroll { get; set; }

		public int ScrollIndex { get; private set; }

		public bool ScrollTo (int index, Action callback)
		{
			var rows = _view.IndexPathsForVisibleRows;
			if (rows.Length > 0) {
				int minIndex = rows [0].Row;
				int maxIndex = rows [rows.Length - 1].Row;
				if (index > minIndex && index < maxIndex) {
					callback ();
					return true;
				}
			}

			if ((DateTime.Now - _scrollAnimationFinished).TotalMilliseconds > 400) {
				_scrollEndedCallback = callback;
				_hideOverlaysAfterScrolling = false;
				Index = index;
				return true;
			}
			return false;
		}

		public override UIView CreateView ()
		{
			_view = new NativeTableView (this);
			_view.Source = new NativeTableViewSource (this);
			_view.SeparatorStyle = UITableViewCellSeparatorStyle.None;

			return _view;
		}

		public override Bound Apply (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			base.Apply (stylesheet, styleBound, maxBound);

			this._stylesheet = stylesheet;

			// background color
			_view.BackgroundColor = stylesheet.GetHelper<StyleHelper> ().ColorOrClear<BackgroundColor> (this);

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

		#region IApplicationContextAware implementation

		public void SetApplicationContext (object applicationContext)
		{
			_applicationContext = (ApplicationContext)applicationContext;
		}

		#endregion

		#region IPersistable implementation

		public object GetState ()
		{
			return _view.ContentOffset;
		}

		public void SetState (object state)
		{
			if (state is PointF)
				_scrollOffset = (PointF)state;
		}

		#endregion

		protected override void Dispose (bool disposing)
		{
			if (!_disposed) {
				foreach (var control in _controls)
					control.Dispose ();				
				_controls.Clear ();
				_controls = null;

				if (_view != null && _view.Source != null) {
					var source = _view.Source;
					_view.Source = null;
					source.Dispose ();
				}

				_disposed = true;
			}

			base.Dispose (disposing);
		}

		protected override void OnSetFrame ()
		{
			if (_scrollOffset.HasValue)
				_view.SetContentOffset (_scrollOffset.Value, false);
			else
				SetIndex ();
		}

		void SetIndex ()
		{
			if (_controls.Count > 0) {
				int realIndex;
				if (_scrollIndex >= _controls.Count)
					realIndex = _controls.Count > 0 ? _controls.Count - 1 : 0;
				else if (_scrollIndex < 0)
					realIndex = 0;
				else
					realIndex = _scrollIndex;

				bool isShown = _view.Window != null;
				NSIndexPath path = NSIndexPath.FromRowSection (realIndex, 0);
				_view.ScrollToRow (path, UITableViewScrollPosition.Top, isShown);
			}
		}

		void HideKeyboard ()
		{
			if (_hideOverlaysAfterScrolling) {
				CloseModalWindows ();
				EndEditing ();
			}
		}

		UITableViewCell GetCell (int position)
		{
			var control = GetView (position);

			UITableViewCell cell = new UITableViewCell ();
			cell.ContentView.AddSubview (control.View);
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			cell.BackgroundColor = _view.BackgroundColor;

			return cell;
		}

		IControl<UIView> GetView (int position)
		{
			IControl<UIView> control = _controls [position];
			if (control.View == null) {
				control.CreateView ();
				LayoutChild (control);
			}
			return control;
		}

		void LayoutChild (IControl<UIView> control)
		{
			StyleHelper style = _stylesheet.GetHelper<StyleHelper> ();

			float parentWidth = _view.Frame.Width;
			float parentHeight = _view.Frame.Height;
					    
			float w = style.Width (control, parentWidth);
			float h = style.Height (control, parentHeight);

			LayoutBehaviour.InitializeImageContainer (_stylesheet, control, ref w, ref h);

			Bound bound = control.Apply (_stylesheet, new Bound (w, h), new Bound (w, float.MaxValue));

			control.Frame = new Rectangle (0, 0, bound);
		}

		void OnScrollInvoke (int index)
		{
			ScrollIndex = index;

			if (OnScroll != null) {
				OnScroll.Execute ();
			}
		}

		public class NativeTableView: UITableView
		{
			ScrollView _controller;
			PointF _lastPoint;

			public NativeTableView (ScrollView controller)
			{
				_controller = controller;
			}

			public override UIView HitTest (PointF point, UIEvent uievent)
			{
				UIView hitView = base.HitTest (point, uievent);
				if (_controller == null || _controller._disposed)
					return hitView;				

				if (hitView != null && point != _lastPoint && hitView.Superview != null && hitView.Superview.Superview != null && hitView.Superview.Superview.Superview != null) {

					UIView view = hitView;

					while (!(view.Superview.Superview.Superview is UITableViewCell)) {
						view = view.Superview;
						if (view.Superview.Superview.Superview == null)
							return hitView;						
					}

					int index = -1;
					for (int i = 0; i < _controller._controls.Count; i++) {
						UIView cview = _controller._controls [i].View;
						if (view.Equals (cview)) {
							index = i;
							break;
						}
					}

					_controller.OnScrollInvoke (index);
					_lastPoint = point;
				}
				return hitView;
			}

			public override void MovedToWindow ()
			{
				base.MovedToWindow ();

				SetContentOffset (new PointF(ContentOffset.X, ContentOffset.Y + 1), true);
			}

			protected override void Dispose (bool disposing)
			{
				base.Dispose (disposing);

				if (_controller != null)
					_controller = null;
			}
		}

		public class NativeTableViewSource: UITableViewSource
		{
			ScrollView _controller;

			public NativeTableViewSource (ScrollView controller)
			{
				_controller = controller;
			}

			public override void Scrolled (UIScrollView scrollView)
			{	
				if (_controller == null || _controller._disposed)
					return;

				_controller.HideKeyboard ();
			}

			public override void ScrollAnimationEnded (UIScrollView scrollView)
			{
				if (_controller == null || _controller._disposed)
					return;
				
				PointF offset = scrollView.ContentOffset;
				if (_controller._scrollEndedCallback != null) {
					_controller._scrollEndedCallback ();
					_controller._scrollEndedCallback = null;

					_controller._hideOverlaysAfterScrolling = true;
				}

				_controller._scrollAnimationFinished = DateTime.Now;
			}

			public override int RowsInSection (UITableView tableview, int section)
			{
				if (_controller == null || _controller._disposed)
					return 0;
				return _controller._controls.Count;
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				if (_controller == null || _controller._disposed)
					return new UITableViewCell ();
				return _controller.GetCell (indexPath.Row);
			}

			public override float EstimatedHeight (UITableView tableView, NSIndexPath indexPath)
			{
				return UITableView.AutomaticDimension;
			}

			public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				if (_controller == null || _controller._disposed)
					return 0;
				return _controller.GetView (indexPath.Row).Frame.Height;
			}

			protected override void Dispose (bool disposing)
			{
				_controller = null;

				base.Dispose (disposing);
			}
		}
	}
}

