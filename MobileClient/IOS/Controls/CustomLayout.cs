using System;
using System.Collections.Generic;
using System.Linq;
using BitMobile.Application.Controls;
using BitMobile.Application.Log;
using BitMobile.Application.StyleSheet;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.IOS;
using BitMobile.UI;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace BitMobile.Controls
{
    public abstract class CustomLayout : Control<CustomLayout.NativeView>, ILayoutableContainer, IValidatable
    {
        protected readonly ILayoutableContainerBehaviour<Control> ContainerBehaviour;
        private string _onEvent = "null";
        private UIColor _backgroundColor;
        private UIImage _backgroundImage;
        private UIColor _selectedColor;
        private UIImage _selectedImage;

        protected CustomLayout()
        {
            ContainerBehaviour = ControlsContext.Current.CreateContainerBehaviour<Control>(this);
        }

        public IActionHandler OnClickAction { get; set; }

        public IActionHandlerEx OnClick { get; set; }

        public String OnEvent
        {
            get { return _onEvent; }
            set
            {
                _onEvent = value;
                CurrentContext.SubscribeEvent(value, InvokeClickAction);
            }
        }

        public string SubmitScope { get; set; }

        // ReSharper disable once InconsistentNaming
        public object append(string xml)
        {
            Inject(ContainerBehaviour.Childrens.Count, xml);
            return this;
        }

        // ReSharper disable once InconsistentNaming
        public object prepend(string xml)
        {
            Inject(0, xml);
            return this;
        }

        public override void CreateView()
        {
            _view = new NativeView();
            _view.TouchesBeganEvent += HandleTouchesBeganEvent;
            _view.TouchesEndedEvent += HandleTouchesEndedEvent;
            _view.TouchesCancelledEvent += HandleTouchesCancelledEvent;

            CreateChildrens();
        }

        public override void AnimateTouch(TouchEventType touch)
        {
            switch (touch)
            {
                case TouchEventType.Begin:

                    if (_selectedImage != null)
                        DrawBackgroundImage(_selectedImage);
                    else if (_selectedColor != null)
                        _view.BackgroundColor = _selectedColor;

                    break;
                case TouchEventType.Cancel:
                case TouchEventType.End:

                    if (_selectedImage != null)
                        DrawBackgroundImage(_backgroundImage);
                    else if (_selectedColor != null)
                    {
                        UIView.BeginAnimations(null);
                        UIView.SetAnimationDuration(0.1);
                        _view.BackgroundColor = _backgroundColor;
                        UIView.CommitAnimations();
                    }
                    break;
            }

            foreach (Control control in ContainerBehaviour.Childrens)
                control.AnimateTouch(touch);
        }

        #region IContainer

        public virtual void AddChild(object obj)
        {
            Insert(ContainerBehaviour.Childrens.Count, obj);
        }

        public object[] Controls
        {
            get { return ContainerBehaviour.Childrens.ToArray(); }
        }

        public object GetControl(int index)
        {
            return ContainerBehaviour.Childrens[index];
        }
        #endregion

        #region ILayoutableContainer

        public void Insert(int index, object obj)
        {
            ContainerBehaviour.Insert(index, obj);
        }

        public void Withdraw(int index)
        {
            if (ContainerBehaviour.Childrens.Count > index)
            {
                UIView view = ContainerBehaviour.Childrens[index].View;
                if (view != null)
                    view.RemoveFromSuperview();
            }

            ContainerBehaviour.Withdraw(index);
        }

        public void Inject(int index, string xml)
        {
            ContainerBehaviour.Inject(index, xml);
        }

        public void CreateChildrens()
        {
            foreach (Control control in ContainerBehaviour.Childrens)
                if (control.View == null)
                {
                    control.CreateView();
                    _view.AddSubview(control.View);
                }
        }
        #endregion

        #region IValidatable

        public bool Validate()
        {
            bool result = true;

            foreach (Control control in ContainerBehaviour.Childrens)
            {
                var validatable = control as IValidatable;
                if (validatable != null)
                    result &= validatable.Validate();
            }

            return result;
        }

        #endregion

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            IStyleSheetHelper style = stylesheet.Helper;

            // background color, background image, borders
            _backgroundImage = stylesheet.SetBackgroundSettings(this);
            if (_backgroundImage == null)
                _backgroundColor = _view.BackgroundColor;

            // selected-color
            _selectedColor = style.SelectedColor(this).ToNullableColor();
            if (_selectedColor != null && _backgroundImage != null)
                _selectedImage = GetFilteredImage(_backgroundImage, _selectedColor);

            // size to content by background
            IBound bound = GetBoundByImage(styleBound, maxBound, _backgroundImage);
            return LayoutChildren(stylesheet, bound, maxBound);
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            IStyleHelper helper = StyleSheetContext.Current.CreateHelper(styles, CurrentStyleSheet, this);

            if (styles.Count > 0)
            {
                // background color, background image, borders
                _backgroundImage = helper.SetBackgroundSettings(this);
                if (_backgroundImage == null)
                    _backgroundColor = _view.BackgroundColor;
                else
                    DrawBackgroundImage(_backgroundImage);
                // selected-color
                _selectedColor = helper.Get<ISelectedColor>().ToNullableColor();
                if (_selectedColor != null && _backgroundImage != null)
                    _selectedImage = GetFilteredImage(_backgroundImage, _selectedColor);
            }
            // size to content by background
            IBound bound = GetBoundByImage(styleBound, maxBound, _backgroundImage);
            return LayoutChildren(CurrentStyleSheet, bound, maxBound);
        }
        protected abstract IBound LayoutChildren(IStyleSheet stylesheet, IBound styleBound, IBound maxBound);

        protected override void Dismiss()
        {
            DisposeField(ref _backgroundColor);
            DisposeField(ref _selectedImage);
            DisposeField(ref _backgroundImage);
            DisposeField(ref _selectedImage);

            _view.TouchesBeganEvent -= HandleTouchesBeganEvent;
            _view.TouchesEndedEvent -= HandleTouchesEndedEvent;
            _view.TouchesCancelledEvent -= HandleTouchesCancelledEvent;

            foreach (Control control in ContainerBehaviour.Childrens)
                control.DismissView();
        }

        protected override void OnSetFrame()
        {
            if (_backgroundImage != null)
                DrawBackgroundImage(_backgroundImage);
        }

        private void HandleTouchesBeganEvent(NSSet touches, UIEvent evt)
        {
            CloseModalWindows();
            if (OnClickAction != null || OnClick != null)
            {
                EndEditing();

                AnimateTouch(TouchEventType.Begin);
            }
            else if (_view.Superview != null)
                _view.Superview.TouchesBegan(touches, evt);
        }

        private void HandleTouchesEndedEvent(NSSet touches, UIEvent evt)
        {
            _view.EndEditing(true);
            if (OnClick != null || OnClickAction != null)
            {
                InvokeClickAction();

                AnimateTouch(TouchEventType.End);
            }
        }

        private void HandleTouchesCancelledEvent(NSSet touches, UIEvent evt)
        {
            _view.EndEditing(true);

            AnimateTouch(TouchEventType.Cancel);

            if (_view.Superview != null)
                _view.Superview.TouchesCancelled(touches, evt);
        }

        private bool InvokeClickAction()
        {
            if (OnClick != null || OnClickAction != null)
            {
                bool allowed = true;
                if (!string.IsNullOrWhiteSpace(SubmitScope))
                    allowed = CurrentContext.Validate(SubmitScope);

                if (allowed)
                {
                    if (OnClick != null)
                    {
                        LogManager.Logger.Clicked(Id, OnClick.Expression);
                        CurrentContext.JokeProviderInternal.OnTap();
                        OnClick.Execute();
                        return true;
                    }

                    if (OnClickAction != null)
                    {
                        LogManager.Logger.Clicked(Id, OnClickAction.Expression);
                        CurrentContext.JokeProviderInternal.OnTap();
                        OnClickAction.Execute();
                        return true;
                    }
                }
            }
            return false;
        }

        private void DrawBackgroundImage(UIImage background)
        {
            UIImage img = null;
            try
            {
                UIGraphics.BeginImageContext(_view.Frame.Size);
                background.Draw(_view.Bounds);
                img = UIGraphics.GetImageFromCurrentImageContext();
                if (img != null)
                    _view.BackgroundColor = UIColor.FromPatternImage(img);
            }
            finally
            {
                UIGraphics.EndImageContext();
                if (img != null)
                    img.Dispose();
            }
        }


        public class NativeView : UIView
        {
            public delegate void TouchesDelegate(NSSet touches, UIEvent evt);

            public event TouchesDelegate TouchesBeganEvent;

            public override void TouchesBegan(NSSet touches, UIEvent evt)
            {
                base.TouchesBegan(touches, evt);
                if (TouchesBeganEvent != null)
                    TouchesBeganEvent(touches, evt);
            }

            public event TouchesDelegate TouchesEndedEvent;

            public override void TouchesEnded(NSSet touches, UIEvent evt)
            {
                base.TouchesEnded(touches, evt);
                if (TouchesEndedEvent != null)
                    TouchesEndedEvent(touches, evt);
            }

            public event TouchesDelegate TouchesCancelledEvent;

            public override void TouchesCancelled(NSSet touches, UIEvent evt)
            {
                base.TouchesCancelled(touches, evt);
                if (TouchesCancelledEvent != null)
                    TouchesCancelledEvent(touches, evt);
            }
        }
    }
}