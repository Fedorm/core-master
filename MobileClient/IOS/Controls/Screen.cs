using System;
using System.Collections.Generic;
using System.Drawing;
using BitMobile.Application.Controls;
using BitMobile.Application.StyleSheet;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.IOS;
using BitMobile.UI;
using MonoTouch.UIKit;

namespace BitMobile.Controls
{
    [Synonym("body")]
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "Screen")]
    public class Screen : Control<UIView>, ILayoutableContainer, IScreen, ICustomStyleSheet, IValidatable
    {
        private readonly ILayoutableContainerBehaviour<Control> _containerBehaviour;

        public Screen()
        {
            _containerBehaviour = ControlsContext.Current.CreateContainerBehaviour<Control>(this);
        }

        public IActionHandlerEx OnLoading
        {
            set
            {
                if (value != null)
                    value.Execute();
            }
        }

        public IActionHandlerEx OnLoad { get; set; }

        public override void CreateView()
        {
            _view = new UIView();
            CreateChildrens();
        }

        #region IContainer

        public void AddChild(object obj)
        {
            Insert(0, obj);
        }

        public object[] Controls
        {
            get
            {
                Control child = GetChild();
                if (child != null)
                    return new object[] { child };
                return new object[0];
            }
        }

        public object GetControl(int index)
        {
            return GetChild();
        }

        #endregion

        #region ILayoutableContainer

        public void Insert(int index, object obj)
        {
            if (_containerBehaviour.Childrens.Count == 0)
                _containerBehaviour.Insert(0, obj);
            else
                throw new Exception("Only one child is allowed");
        }

        public void Withdraw(int index)
        {
            if (_containerBehaviour.Childrens.Count > index)
            {
                UIView view = _containerBehaviour.Childrens[index].View;
                if (view != null)
                    view.RemoveFromSuperview();
            }
            _containerBehaviour.Withdraw(index);
        }

        public void Inject(int index, string xml)
        {
            if (_containerBehaviour.Childrens.Count == 0)
                _containerBehaviour.Inject(0, xml);
            else
                throw new Exception("Only one child is allowed");
        }

        public void CreateChildrens()
        {
            Control child = GetChild();
            if (child != null)
            {
                child.CreateView();
                _view.AddSubview(child.View);
            }
        }
        #endregion

        #region IScreen

        public void ExitEditMode()
        {
            _view.EndEditing(true);
        }

        #endregion

        #region ICustomStyleSheet

        public string StyleSheet { get; set; }

        #endregion

        #region IValidatable

        public bool Validate()
        {
            bool result = true;

            var validatable = GetChild() as IValidatable;
            if (validatable != null)
                result &= validatable.Validate();


            return result;
        }

        #endregion

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            // background color
            _view.BackgroundColor = stylesheet.Helper.BackgroundColor(this).ToColorOrClear();

            IBound bound = ApplyChild(stylesheet);

            RectangleF app = UIScreen.MainScreen.ApplicationFrame;
            Frame = ControlsContext.Current.CreateRectangle(app.Left, app.Top, bound);

            if (OnLoad != null)
                OnLoad.Execute();

            return bound;
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            IStyleHelper helper = StyleSheetContext.Current.CreateHelper(styles, CurrentStyleSheet, this);

            if (styles.Count > 0)
            {
                IBackgroundColor backgroundColor;
                if (helper.TryGet(out backgroundColor))
                    _view.BackgroundColor = backgroundColor.ToColorOrClear();
            }

            return ApplyChild(CurrentStyleSheet);
        }

        protected override void Dismiss()
        {
            Control child = GetChild();
            if (child != null)
                child.DismissView();
        }

        private IBound ApplyChild(IStyleSheet stylesheet)
        {
            IControlsContext context = ControlsContext.Current;
            RectangleF app = UIScreen.MainScreen.ApplicationFrame;
            IBound bound = StyleSheetContext.Current.CreateBound(app.Width, app.Height);

            Control child = GetChild();
            if (child != null)
            {
                context.CreateLayoutBehaviour(stylesheet, this).Screen(child, bound);

                IRectangle old = child.Frame;
                child.Frame = context.CreateRectangle(old.Left + app.Left, old.Top + app.Top, old.Width, old.Height);
            }
            return bound;
        }

        private Control GetChild()
        {
            if (_containerBehaviour.Childrens.Count == 1)
                return _containerBehaviour.Childrens[0];
            return null;
        }
    }
}