using System;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Views;
using BitMobile.Application.StyleSheet;
using BitMobile.Common.StyleSheet;
using BitMobile.Droid.StyleSheet;
using BitMobile.Droid.UI;

namespace BitMobile.Droid.Controls
{
    public abstract class CustomText<T> : Control<T>
        where T : Android.Widget.TextView
    {
        protected TextAlignValues DefaultAlignValues;
        private TextFormatValues _textFormat;
        private Color? _selectedColor;
        private SelectionBehaviour _selectionBehaviour;
        private Color _textColor;
        private bool _singleLine;
        private string _text = "";

        protected CustomText(BaseScreen activity)
            : base(activity)
        {
            DefaultAlignValues = TextAlignValues.Left;
        }

        // ReSharper disable once UnusedMember.Global
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value ?? string.Empty;
                if (_view != null)
                    SetText();
            }
        }        

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            var style = stylesheet.Helper;

            // text-format
            _textFormat = style.TextFormat(this);
            SetText();

            // background color, background image, borders
            using (var background = stylesheet.Background(this, styleBound))
                SetBackground(background);

            // font
            stylesheet.SetFontSettings(this, _view, styleBound.Height);

            // text color
            _textColor = style.Color(this).ToColorOrTransparent();
            _view.SetTextColor(_textColor);

            //selected color
            _selectedColor = style.SelectedColor(this).ToNullableColor();

            //selected background
            _selectionBehaviour = new SelectionBehaviour(style.SelectedBackground(this).ToNullableColor(), this, stylesheet);

            // word wrap
            _singleLine = style.WhiteSpace(this) == WhiteSpaceKind.Nowrap;
            if (_singleLine)
                _view.SetSingleLine();

            // text align
            TextAlignValues align = style.TextAlign(this, DefaultAlignValues);
            ApplyTextAlign(align);
            if (_singleLine)
                _view.Ellipsize = align == TextAlignValues.Right ? TextUtils.TruncateAt.Start : TextUtils.TruncateAt.End;

            // text padding
            int pl = style.PaddingLeft(this, styleBound.Width).Round();
            int pt = style.PaddingTop(this, styleBound.Height).Round();
            int pr = style.PaddingRight(this, styleBound.Width).Round();
            int pb = style.PaddingBottom(this, styleBound.Height).Round();
            _view.SetPadding(pl, pt, pr, pb);

            return SizeToContent(styleBound, maxBound, style);
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            IStyleHelper helper = StyleSheetContext.Current.CreateHelper(styles, CurrentStyleSheet, this);

            if (styles.Count > 0)
            {
                // text-format
                ITextFormat textFormat;
                if (helper.TryGet(out textFormat))
                {
                    _textFormat = textFormat.Format;
                    SetText();
                }

                // background color, background image, borders
                Drawable background;
                if (helper.BackgroundChanged(CurrentStyleSheet, Frame.Bound, out background))
                    using (background)
                        SetBackground(background);

                // font
                helper.SetFontSettings(_view, styleBound.Height);

                // text color
                ITextColor textColor;
                if (helper.TryGet(out textColor))
                {
                    _textColor = textColor.ToColorOrTransparent();
                    _view.SetTextColor(_textColor);
                }

                //selected color
                ISelectedColor selectedColor;
                if (helper.TryGet(out selectedColor))
                    _selectedColor = selectedColor.ToNullableColor();

                //selected background
                ISelectedBackground selectedBackground;
                if (helper.TryGet(out selectedBackground))
                    _selectionBehaviour.SelectedColor = selectedBackground.ToNullableColor();

                // word wrap
                IWhiteSpace whiteSpace;
                if (helper.TryGet(out whiteSpace))
                {
                    _singleLine = whiteSpace.Kind == WhiteSpaceKind.Nowrap;
                    _view.SetSingleLine(_singleLine);
                }

                // text align
                ITextAlign textAlign;
                if (helper.TryGet(out textAlign))
                {
                    ApplyTextAlign(textAlign.Align);
                    if (_singleLine)
                        _view.Ellipsize = textAlign.Align == TextAlignValues.Right
                            ? TextUtils.TruncateAt.Start
                            : TextUtils.TruncateAt.End;
                }

                // text padding
                int pl = helper.GetSizeOrDefault<IPaddingLeft>(styleBound.Width, _view.PaddingLeft).Round();
                int pt = helper.GetSizeOrDefault<IPaddingTop>(styleBound.Height, _view.PaddingTop).Round();
                int pr = helper.GetSizeOrDefault<IPaddingRight>(styleBound.Width, _view.PaddingRight).Round();
                int pb = helper.GetSizeOrDefault<IPaddingBottom>(styleBound.Height, _view.PaddingBottom).Round();
                _view.SetPadding(pl, pt, pr, pb);
            }

            IBound bound = SizeToContent(styleBound, maxBound, CurrentStyleSheet.Helper);

            // reload image with new size
            helper.ReloadBackgroundImage(CurrentStyleSheet, bound, this);

            return bound;
        }
        
        protected virtual void ApplyTextAlign(TextAlignValues textAlign)
        {
            switch (textAlign)
            {
                case TextAlignValues.Left:
                    _view.Gravity = GravityFlags.Left | GravityFlags.Top;
                    break;
                case TextAlignValues.Center:
                    _view.Gravity = GravityFlags.Center | GravityFlags.Top;
                    break;
                case TextAlignValues.Right:
                    _view.Gravity = GravityFlags.Right | GravityFlags.Top;
                    break;
            }
        }

        protected override void Dismiss()
        {            
            DisposeField(ref _selectionBehaviour);
            base.Dismiss();
        }

        protected void DoTouchAnimation(MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    if (_selectedColor != null)
                        _view.SetTextColor(_selectedColor.Value);

                    _selectionBehaviour.AnimateDown();
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    if (_selectedColor != null)
                        _view.SetTextColor(_textColor);

                    _selectionBehaviour.AnimateUp();
                    break;
            }
        }

        private void SetText()
        {
            switch (_textFormat)
            {
                case TextFormatValues.Text:
                    _view.Text = _text;
                    break;
                case TextFormatValues.Html:
                    _view.SetText(Html.FromHtml(_text), Android.Widget.TextView.BufferType.Spannable);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IBound SizeToContent(IBound styleBound, IBound maxBound, IStyleSheetHelper style)
        {
            IBound bound = GetBoundByBackgroud(styleBound, maxBound);

            bool sizeByWith = style.SizeToContentWidth(this);
            bool sizeByHeight = style.SizeToContentHeight(this);
            if (sizeByWith || sizeByHeight)
            {
                float measureWidth = sizeByWith ? maxBound.Width : styleBound.Width;
                float measureHeight = sizeByHeight ? maxBound.Height : styleBound.Height;

                // RedundantNameQualifier to fix compile error
                // ReSharper disable RedundantNameQualifier
                int wspec = Android.Views.View.MeasureSpec.MakeMeasureSpec(Java.Lang.Math.Round(measureWidth), MeasureSpecMode.AtMost);
                int hspec = Android.Views.View.MeasureSpec.MakeMeasureSpec(Java.Lang.Math.Round(measureHeight), MeasureSpecMode.AtMost);
                // ReSharper restore RedundantNameQualifier

                _view.Measure(wspec, hspec);
                float w = sizeByWith ? _view.MeasuredWidth : styleBound.Width;
                float h = sizeByHeight ? _view.MeasuredHeight : styleBound.Height;

                // choose max bound: from image resizing or from size to content by text
                bool safeProportion = !bound.Equals(styleBound) && sizeByWith && sizeByHeight;
                bound = StyleSheetContext.Current.MergeBound(bound, w, h, maxBound, safeProportion);
            }
            return bound;
        }
    }
}