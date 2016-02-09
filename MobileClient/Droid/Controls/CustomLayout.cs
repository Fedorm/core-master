using System;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics.Drawables;
using Android.Views;
using BitMobile.Application.Controls;
using BitMobile.Application.Log;
using BitMobile.Application.StyleSheet;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.Droid.StyleSheet;
using BitMobile.Droid.UI;

namespace BitMobile.Droid.Controls
{
    // ReSharper disable MemberCanBeProtected.Global, MemberCanBePrivate.Global, UnusedMember.Global, UnusedAutoPropertyAccessor.Global
    public abstract class CustomLayout : Control<CustomViewGroup>, ILayoutableContainer, IValidatable
    {
        protected readonly ILayoutableContainerBehaviour<Control> ContainerBehaviour;
        private bool _pressed;
        private bool _clickable;
        private IActionHandler _onClickAction;
        private IActionHandlerEx _onClick;
        private string _onEvent;
        private SelectionBehaviour _selectionBehaviour;
        
        protected CustomLayout(BaseScreen activity)
            : base(activity)
        {
            ContainerBehaviour = ControlsContext.Current.CreateContainerBehaviour<Control>(this);
        }

        public IActionHandler OnClickAction
        {
            get { return _onClickAction; }
            set
            {
                _onClickAction = value;
                SetClickable(value);
            }
        }

        public IActionHandlerEx OnClick
        {
            get { return _onClick; }
            set
            {
                _onClick = value;
                SetClickable(value);
            }
        }

        public String OnEvent
        {
            get
            {
                return _onEvent;
            }
            set
            {
                BitBrowserApp.Current.SubscribeEvent(value, InvokeClickAction);
                _onEvent = value;
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
            _view = new CustomViewGroup(Activity);
            _view.TouchEvent += View_TouchInvoke;
            _view.TouchingEvent += View_TouchingInvoke;
            _view.LayoutEvent += View_LayoutInvoke;
            _view.MeasureEvent += View_MeasureInvoke;
            _view.Clickable = _clickable;
            _view.Focusable = _clickable;

            CreateChildrens();
        }

        public sealed override void AnimateTouch(MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    _selectionBehaviour.AnimateDown();

                    foreach (var control in ContainerBehaviour.Childrens)
                        control.AnimateTouch(e);

                    _pressed = true;
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    if (_pressed)
                    {
                        _selectionBehaviour.AnimateUp();

                        foreach (var control in ContainerBehaviour.Childrens)
                            control.AnimateTouch(e);

                        _pressed = false;
                    }
                    break;
            }
        }

        #region IContainer

        public object[] Controls
        {
            get
            {
                // ReSharper disable once CoVariantArrayConversion
                return ContainerBehaviour.Childrens.ToArray();
            }
        }

        public virtual void AddChild(object obj)
        {
            Insert(ContainerBehaviour.Childrens.Count, obj);
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
            ContainerBehaviour.Withdraw(index);
            _view.RemoveViewAt(index);
        }

        public void Inject(int index, string xml)
        {
            ContainerBehaviour.Inject(index, xml);
        }

        public void CreateChildrens()
        {
            for (int i = 0; i < ContainerBehaviour.Childrens.Count; i++)
            {
                Control control = ContainerBehaviour.Childrens[i];
                if (control.View == null)
                {
                    control.CreateView();
                    _view.AddView(control.View, i);
                }
            }
        }

        #endregion

        #region IValidatable

        public bool Validate()
        {
            bool result = true;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (Control children in ContainerBehaviour.Childrens)
            {
                var validatable = children as IValidatable;
                if (validatable != null)
                    result &= validatable.Validate();
            }
            return result;
        }

        #endregion

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            var style = stylesheet.Helper;

            // background color, background image, borders
            using (var background = stylesheet.Background(this, styleBound))
                SetBackground(background);

            // selected color
            _selectionBehaviour = new SelectionBehaviour(style.SelectedColor(this).ToNullableColor(), this, stylesheet);

            IBound bound = GetBoundByBackgroud(styleBound, maxBound);

            return LayoutChildren(stylesheet, bound, maxBound);
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            IStyleHelper helper = StyleSheetContext.Current.CreateHelper(styles, CurrentStyleSheet, this);

            if (styles.Count > 0)
            {
                // background color, background image, borders
                Drawable background;
                if (helper.BackgroundChanged(CurrentStyleSheet, Frame.Bound, out background))
                    using (background)
                        SetBackground(background);

                // selected color
                ISelectedColor selectedColor;
                if (helper.TryGet(out selectedColor))
                    _selectionBehaviour.SelectedColor = selectedColor.ToNullableColor();
            }

            IBound bound = GetBoundByBackgroud(styleBound, maxBound);
            return LayoutChildren(CurrentStyleSheet, bound, maxBound);
        }

        protected abstract IBound LayoutChildren(IStyleSheet stylesheet, IBound styleBound, IBound maxBound);

        protected virtual void View_TouchingInvoke(object sender, View.TouchEventArgs e)
        {
            e.Handled = false;
        }

        protected virtual void View_TouchInvoke(object sender, View.TouchEventArgs e)
        {
            if (_clickable)
            {
                e.Handled = true;

                if (_onClickAction != null || _onClick != null)
                {
                    if (e.Event.Action == MotionEventActions.Down && !EditableExist(this))
                        Activity.HideSoftInput();

                    if (_pressed && e.Event.Action == MotionEventActions.Up)
                        if (!CurrentContext.CurrentNativeScreen.GestureHolded())
                            InvokeClickAction();

                    AnimateTouch(e.Event);
                }
            }
            else
                e.Handled = false;
        }

        protected sealed override void Dismiss()
        {            
            foreach (var control in ContainerBehaviour.Childrens)
                control.DismissView();

            DisposeField(ref _selectionBehaviour);
            base.Dismiss();
        }

        protected virtual void View_LayoutInvoke(bool changed, int l, int t, int r, int b)
        {
            foreach (IControl<View> control in ContainerBehaviour.Childrens)
            {
                var left = (int)Math.Round(control.Frame.Left);
                var top = (int)Math.Round(control.Frame.Top);
                var right = (int)Math.Round(control.Frame.Right);
                var bottom = (int)Math.Round(control.Frame.Bottom);

                var width = (int)Math.Round(control.Frame.Width);
                var height = (int)Math.Round(control.Frame.Height);

                control.View.LayoutParameters = new CustomViewGroup.LayoutParams(width, height, left, top);
                control.View.Layout(left, top, right, bottom);
                control.View.RequestLayout();
            }
        }

        private void View_MeasureInvoke(int widthMeasureSpec, int heightMeasureSpec)
        {
            foreach (IControl<View> control in ContainerBehaviour.Childrens)
            {
                var width = (int)Math.Round(control.Frame.Width);
                var height = (int)Math.Round(control.Frame.Height);

                // RedundantNameQualifier to fix compile error
                // ReSharper disable RedundantNameQualifier
                int wspec = Android.Views.View.MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly);
                int hspec = Android.Views.View.MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly);
                // ReSharper restore RedundantNameQualifier
                control.View.Measure(wspec, hspec);
            }
        }

        private bool InvokeClickAction()
        {
            if (_onClick != null || _onClickAction != null)
            {
                bool allowed = true;
                if (!string.IsNullOrWhiteSpace(SubmitScope))
                    allowed = CurrentContext.Validate(SubmitScope);
                if (allowed)
                {
                    if (_onClick != null)
                    {
                        LogManager.Logger.Clicked(Id, _onClick.Expression);
                        CurrentContext.JokeProviderInternal.OnTap();
                        _view.PlaySoundEffect(SoundEffects.Click);
                        _onClick.Execute();
                        return true;
                    }

                    if (_onClickAction != null)
                    {
                        LogManager.Logger.Clicked(Id, _onClickAction.Expression);
                        CurrentContext.JokeProviderInternal.OnTap();
                        _view.PlaySoundEffect(SoundEffects.Click);
                        _onClickAction.Execute();
                        return true;
                    }
                }
            }
            return false;
        }

        private void SetClickable(object value)
        {
            _clickable = value != null;
            if (View != null)
                View.Clickable = value != null;
        }


        private static bool EditableExist(IContainer continer)
        {
            foreach (var item in continer.Controls)
            {
                if (item is EditText || item is MemoEdit)
                    return true;

                var subc = item as IContainer;
                if (subc != null && EditableExist(subc))
                    return true;
            }

            return false;
        }
    }
}
