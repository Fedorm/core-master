using System;
using Android.Text;
using Android.Views;
using BitMobile.Application;
using BitMobile.Controls.StyleSheet;
using BitMobile.Droid;
using BitMobile.Droid.UI;
using Math = Java.Lang.Math;

namespace BitMobile.Controls
{
    public class TextView : Control<Android.Widget.TextView>, IApplicationContextAware, IImageContainer
    {
        private IApplicationContext _applicationContext;
        private TextFormat.Format _textFormat;
        private Android.Graphics.Color? _selectedColor;
        private Android.Graphics.Color _textColor;
        private string _text = "";

        public TextView(BaseScreen activity)
            : base(activity)
        {
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
                _text = value;
                if (_view != null)
                    SetText();
            }
        }

        public override View CreateView()
        {
            _view = new Android.Widget.TextView(_activity);
            _view.SetIncludeFontPadding(false);

            return _view;
        }

        public override Bound Apply(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            var style = stylesheet.GetHelper<StyleHelper>();

            // text-format
            _textFormat = style.TextFormat(this);
            SetText();

            // background color, borders
            var background = style.Background(this, _applicationContext, true);
            _view.SetBackgroundDrawable(background);

            // font
            style.SetFontSettings(this, _view, styleBound.Height);

            // text color
            _textColor = style.ColorOrTransparent<Color>(this);
            _view.SetTextColor(_textColor);

            //selected color
            _selectedColor = style.Color<SelectedColor>(this);

            // word wrap
            bool nowrap = style.WhiteSpaceKind(this) == WhiteSpace.Kind.Nowrap;
            if (nowrap)
                _view.SetSingleLine();

            // text align
            switch (style.TextAlign(this))
            {
                case TextAlign.Align.Left:
                    _view.Gravity = GravityFlags.Left | GravityFlags.Top;
                    if (nowrap)
                        _view.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    break;
                case TextAlign.Align.Center:
                    _view.Gravity = GravityFlags.Center | GravityFlags.Top;
                    if (nowrap)
                        _view.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    break;
                case TextAlign.Align.Right:
                    _view.Gravity = GravityFlags.Right | GravityFlags.Top;
                    if (nowrap)
                        _view.Ellipsize = Android.Text.TextUtils.TruncateAt.Start;
                    break;
            }

            // text padding
            int pl = style.Padding<PaddingLeft>(this, styleBound.Width).Round();
            int pt = style.Padding<PaddingTop>(this, styleBound.Height).Round();
            int pr = style.Padding<PaddingRight>(this, styleBound.Width).Round();
            int pb = style.Padding<PaddingBottom>(this, styleBound.Height).Round();
            _view.SetPadding(pl, pt, pr, pb);

            bool sizeByWith = style.SizeToContentWidth(this);
            bool sizeByHeight = style.SizeToContentHeight(this);
            if (sizeByWith || sizeByHeight)
            {
                float measureWidth = sizeByWith ? maxBound.Width : styleBound.Width;
                float measureHeight = sizeByHeight ? maxBound.Height : styleBound.Height;

                int wspec = View.MeasureSpec.MakeMeasureSpec(Math.Round(measureWidth), MeasureSpecMode.AtMost);
                int hspec = View.MeasureSpec.MakeMeasureSpec(Math.Round(measureHeight), MeasureSpecMode.AtMost);

                _view.Measure(wspec, hspec);
                float w = sizeByWith ? _view.MeasuredWidth : styleBound.Width;
                float h = sizeByHeight ? _view.MeasuredHeight : styleBound.Height;
                return new Bound(w, h);
            }
            return styleBound;
        }

        public override void AnimateTouch(MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    if (_selectedColor != null)
                        _view.SetTextColor(_selectedColor.Value);
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    _view.SetTextColor(_textColor);
                    break;
            }
        }

        #region IApplicationContextAware

        public void SetApplicationContext(object applicationContext)
        {
            _applicationContext = (Droid.ApplicationContext)applicationContext;
        }
        #endregion

        #region IImageContainer

        public int ImageWidth { get; set; }

        public int ImageHeight { get; set; }

        public bool InitializeImage(StyleSheet.StyleSheet stylesheet)
        {
            return stylesheet.GetHelper<StyleHelper>().InitializeImageContainer(this, _applicationContext);
        }
        #endregion
        
        private void SetText()
        {
            switch (_textFormat)
            {
                case TextFormat.Format.Text:
                    _view.Text = _text;
                    break;
                case TextFormat.Format.Html:
                    _view.SetText(Html.FromHtml(_text), Android.Widget.TextView.BufferType.Spannable);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
