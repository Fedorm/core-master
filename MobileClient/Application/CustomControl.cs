using System;
using System.Collections.Generic;
using System.Linq;
using BitMobile.Application.Controls;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;

namespace BitMobile.Application
{
    public abstract class CustomControl<T> : IControl<T>
    {

        private IDictionary<Type, IStyle> _styles;
        private bool _applied;
        private bool _disposed;
        private bool _siblingsChanged;

        protected IStyleSheet CurrentStyleSheet { get; private set; }

        // ReSharper disable InconsistentNaming

        public object after(string xml)
        {
            Parent.Inject(IndexInParent() + 1, xml);
            _siblingsChanged = true;
            return this;
        }

        public object before(string xml)
        {
            Parent.Inject(IndexInParent(), xml);
            _siblingsChanged = true;
            return this;
        }

        public object remove()
        {
            Parent.Withdraw(IndexInParent());
            _siblingsChanged = true;
            return this;
        }

        public object refresh()
        {
            Refresh();
            return this;
        }
        // ReSharper restore InconsistentNaming

        public abstract void DismissView();

        #region IStyledObject

        public string Name
        {
            get { return GetType().Name; }
        }

        public string CssClass { get; set; }

        public IBound ApplyStyles(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            if (_applied)
            {
                IDictionary<Type, IStyle> delta = CurrentStyleSheet.StylesIntersection(StylesOld, Styles);
                IBound bound = ReApply(delta, styleBound, maxBound);

                StylesOld = null;
                return bound;
            }
            return Apply(stylesheet, styleBound, maxBound);
        }

        #endregion

        #region ILayoutable

        public string Id { get; set; }

        public virtual IRectangle Frame { get; set; }

        public ILayoutableContainer Parent { get; set; }

        public IStyleKey StyleKey { get; set; }

        public IDictionary<Type, IStyle> Styles
        {
            get { return _styles; }
            set
            {
                if (_styles != value)
                {
                    if (StylesOld == null)
                        StylesOld = _styles;
                    _styles = value;
                }
            }
        }

        public IDictionary<Type, IStyle> StylesOld { get; set; }

        public abstract void CreateView();

        public void Refresh()
        {
            ControlsContext.Current.ActionHandlerLocker.Acquire();
            CurrentStyleSheet.Assign((ILayoutable)ApplicationContext.Current.CurrentScreen.Screen);
            Relayout();
            RefreshView();
            ControlsContext.Current.ActionHandlerLocker.Release();
        }
        #endregion

        #region IControl<T>

        public abstract T View { get; }
        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        protected virtual IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            CurrentStyleSheet = stylesheet;
            _applied = true;
            return styleBound;
        }

        protected abstract IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound);

        protected abstract void RefreshView();

        protected void DisposeField<TField>(ref TField field) where TField : class, IDisposable
        {

        }

        protected void Dispose(bool disposing)
        {
            //todo: dispose fields by reflection
            DismissView();
            _disposed = true;
        }

        private void Relayout()
        {
            IStyleSheetHelper helper = CurrentStyleSheet.Helper;
            bool changed = _disposed
                || _siblingsChanged
                || helper.SizeToContentWidth(this)
                || helper.SizeToContentHeight(this)
                || StylesChanged();

            if (Parent != null && changed)
            {
                var parent = (CustomControl<T>)Parent;
                parent.Relayout();
                _siblingsChanged = false;
            }
            else
                ApplyStyles(CurrentStyleSheet, Frame.Bound, Frame.Bound);
        }

        private int IndexInParent()
        {
            for (int i = 0; i < Parent.Controls.Length; i++)
                if (Parent.Controls[i] == this)
                    return i;

            throw new Exception("Cannot find index in parent");
        }

        private bool StylesChanged()
        {
            if (StylesOld == Styles)
                return false;

            if (Styles == null)
                return true;

            if (StylesOld == null)
                return false;

            return !StylesOld.SequenceEqual(Styles);
        }
    }
}