using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using BitMobile.Common.StyleSheet;
using BitMobile.Droid.StyleSheet;

namespace BitMobile.Droid.UI
{
    class SelectionBehaviour : IDisposable
    {
        private readonly IStyleSheet _stylesheet;
        private Color? _selectedColor;
        private Control _control;
        private Drawable _selectedColorDrawable;
        private Drawable _mainColorDrawable;
        private bool _pressed;

        public SelectionBehaviour(Color? selectedColor, Control control, IStyleSheet stylesheet)
        {
            _control = control;
            _stylesheet = stylesheet;
            SelectedColor = selectedColor;
        }

        public Color? SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                if (value != _selectedColor)
                {
                    _selectedColor = value;

                    if (_pressed)
                        AnimateUp();

                    if (_selectedColorDrawable != null)
                        _selectedColorDrawable.Dispose();

                    if (_selectedColor != null && !(_control.View.Background is BitmapDrawable))
                        _selectedColorDrawable = _stylesheet.ColorWithBorders(_control, _selectedColor.Value);
                }
            }
        }

        public void AnimateDown()
        {
            if (_selectedColorDrawable != null)
            {
                _mainColorDrawable = _control.View.Background;
                _control.SetBackground(_selectedColorDrawable);
            }
            else if (_control.View.Background != null && SelectedColor != null)
                _control.View.Background.SetColorFilter(SelectedColor.Value, PorterDuff.Mode.SrcIn);
            _pressed = true;
        }

        public void AnimateUp()
        {
            if (_mainColorDrawable != null)
                _control.SetBackground(_mainColorDrawable);
            else if (_control.View.Background != null && SelectedColor != null)
                _control.View.Background.ClearColorFilter();
            _pressed = false;
        }

        public void Dispose()
        {
            _control = null;

            if (_selectedColorDrawable != null)
                _selectedColorDrawable.Dispose();
            _selectedColorDrawable = null;

            if (_mainColorDrawable != null)
                _mainColorDrawable.Dispose();
            _mainColorDrawable = null;
        }
    }
}