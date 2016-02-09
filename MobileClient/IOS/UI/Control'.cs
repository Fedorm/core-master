using System;
using System.Drawing;
using BitMobile.Application;
using BitMobile.Application.Controls;
using BitMobile.Application.StyleSheet;
using BitMobile.Common.Controls;
using BitMobile.Common.Log;
using BitMobile.Common.StyleSheet;
using BitMobile.Controls;
using BitMobile.IOS;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;

namespace BitMobile.UI
{
    public abstract class Control<T> : Control
        where T : UIView
    {
        private bool _disposed = false;
        protected T _view;
        protected bool _visible = true;

        public override bool Visible
        {
            get { return _visible; }
            set
            {
                _visible = value;
                if (_view != null)
                    _view.Hidden = !_visible;
            }
        }

        [NonLog]
        public override UIView View
        {
            get { return _view; }
        }

        [NonLog]
        internal T ViewInternal
        {
            get { return _view; }
        }

        public override IRectangle Frame
        {
            get
            {
                return ControlsContext.Current.CreateRectangle(_view.Frame.Left, _view.Frame.Top, _view.Frame.Width,
                    _view.Frame.Height);
            }
            set
            {
                _view.Frame = new RectangleF(value.Left, value.Top, value.Width, value.Height);
                OnSetFrame();
            }
        }

        public override sealed void DismissView()
        {
            if (_view != null)
            {
                Dismiss();
                _view.Dispose();
                _view = null;
            }
        }

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            IBound bound = base.Apply(stylesheet, styleBound, maxBound);

            if (_view == null)
                throw new NullReferenceException("cannot apply styles: View is null.");

            _view.Hidden = !_visible;

            return bound;
        }

        protected abstract void Dismiss();

        protected override void RefreshView()
        {
        }

        protected virtual void OnSetFrame()
        {
        }

        protected void CloseModalWindows()
        {
            DatePicker.CancelCurrent();
        }

        protected void EndEditing()
        {
            ((Screen) ApplicationContext.Current.CurrentScreen.Screen).View.EndEditing(true);
        }

        protected void LayoutOverlay(UIView overlay, SizeF overlaySize, UIView container, PointF viewOffset)
        {
            if (container != null)
            {
                RectangleF overlayFrame;
                if (TryFindOverlayFrame(overlaySize, container.Frame.Size, _view.Frame.Size, viewOffset,
                    out overlayFrame))
                {
                    container.AddSubview(overlay);
                    overlay.Frame = overlayFrame;
                }
                else
                    LayoutOverlay(overlay, overlaySize, container.Superview,
                        new PointF(container.Frame.X + viewOffset.X, container.Frame.Y + viewOffset.Y));
            }
        }

        protected static IBound GetBoundByImage(IBound styleBound, IBound maxBound, UIImage image)
        {
            return image != null
                ? StyleSheetContext.Current.StrechBoundInProportion(styleBound, maxBound, image.Size.Width,
                    image.Size.Height)
                : styleBound;
        }

        protected IBound MergeBoundByContent(IStyleSheetHelper style, IBound bound, IBound maxBound, UIEdgeInsets insets,
            bool safeProportion)
        {
            bool sizeByWidth = style.SizeToContentWidth(this);
            bool sizeByHeight = style.SizeToContentHeight(this);
            if (sizeByWidth || sizeByHeight)
            {
                float measureWidth = sizeByWidth ? maxBound.Width : bound.Width;
                float measureHeight = sizeByHeight ? maxBound.Height : bound.Height;

                SizeF size = _view.SizeThatFits(new SizeF(measureWidth, measureHeight));

                float w = sizeByWidth ? size.Width + insets.Left + insets.Right : bound.Width;
                float h = sizeByHeight ? size.Height + insets.Top + insets.Bottom : bound.Height;

                bound = StyleSheetContext.Current.MergeBound(bound, w, h, maxBound,
                    safeProportion && sizeByWidth && sizeByHeight);
            }
            return bound;
        }

        protected UIImage GetFilteredImage(UIImage image, UIColor color)
        {
            var rect = new RectangleF(0, 0, image.Size.Width, image.Size.Height);
            UIGraphics.BeginImageContext(rect.Size);
            CGContext context = UIGraphics.GetCurrentContext();
            image.Draw(rect);
            context.SetFillColorWithColor(color.CGColor);
            context.SetBlendMode(CGBlendMode.SourceIn);
            context.FillRect(rect);
            UIImage img = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return img ?? image;
        }

        private static bool TryFindOverlayFrame(SizeF overlaySize, SizeF containerSize, SizeF viewSize, PointF viewPoint,
            out RectangleF overlayFrame)
        {
            overlayFrame = new RectangleF();
            if (overlaySize.Width > containerSize.Width || overlaySize.Height > containerSize.Height)
                return false;

            float y = viewPoint.Y - overlaySize.Height;
            if (y < 0)
            {
                y = viewPoint.Y + viewSize.Height;
                if (y + overlaySize.Height > containerSize.Height)
                    return false;
            }

            float x = viewPoint.X + viewSize.Width - overlaySize.Width;
            if (x < 0)
            {
                x = viewPoint.X;
                if (x + overlaySize.Width > containerSize.Width)
                    return false;
            }

            overlayFrame = new RectangleF(new PointF(x, y), overlaySize);
            return true;
        }
    }
}