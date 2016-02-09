using System;
using System.Collections.Generic;
using System.Threading;
using BitMobile.Application;
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
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "Button")]
    public class Button : Control<UIButton>
    {        
        private bool _blinkAllowed = true;
        private UIColor _mainBackground;
        private string _onEvent = "null";
        private UIColor _selectedBackground;
        private string _selectedHtmlSpan;
        private string _text;
        private UIColor _textColor;
        private TextFormatValues _textFormat;
        private string _textHtmlSpan;

        public IActionHandler OnClickAction { get; set; }

        public IActionHandlerEx OnClick { get; set; }

        public String Text
        {
            get { return _text; }
            set
            {
                _text = value;
                if (_view != null)
                    SetText();
            }
        }

        public String OnEvent
        {
            get { return _onEvent; }
            set
            {
                _onEvent = value;
                CurrentContext.SubscribeEvent(value, InvokeClick);
            }
        }

        public string SubmitScope { get; set; }

        public override void CreateView()
        {
            _view = new UIButton(UIButtonType.Custom);
            _view.TouchUpInside += HandleTouchUpInside;
            _view.TouchUpOutside += HandleTouchUpOutside;
            _view.TouchDown += HandleTouchDown;
            _view.Enabled = OnClick != null || OnClickAction != null;
        }

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            IStyleSheetHelper style = stylesheet.Helper;

            // background color, borders, background image
            UIImage backgroundImage = stylesheet.SetBackgroundSettings(this);
            if (backgroundImage != null)
                _view.SetBackgroundImage(backgroundImage, UIControlState.Normal);

            // font
            UIFont f = stylesheet.Font(this, styleBound.Height);
            if (f != null)
                _view.Font = f;

            // word wrap
            bool nowrap = style.WhiteSpace(this) == WhiteSpaceKind.Nowrap;
            _view.TitleLabel.LineBreakMode = !nowrap ? UILineBreakMode.WordWrap : UILineBreakMode.TailTruncation;

            //  text-format
            _textFormat = style.TextFormat(this);

            switch (_textFormat)
            {
                case TextFormatValues.Text:
                    SetText();

                    // text color
                    _textColor = style.Color(this).ToColorOrClear();
                    _view.SetTitleColor(_textColor, UIControlState.Normal);

                    // selected-color
                    using (UIColor selectedColor = style.SelectedColor(this).ToNullableColor())
                        if (selectedColor != null)
                            _view.SetTitleColor(selectedColor, UIControlState.Highlighted);

                    break;
                case TextFormatValues.Html:
                    string whitespace = nowrap ? "nowrap" : "normal";
                    string span =
                        string.Format(
                            "<span style=\"font-family: {0}; font-size: {1:F0}; color: {2}; white-space: {3}\">{4}</span>"
                            , _view.Font.FamilyName, _view.Font.PointSize, "{0}", whitespace, "{1}");
                    // text color
                    _textHtmlSpan = string.Format(span, style.Color(this).Hex, "{0}");

                    // selected-color
                    IColorInfo selectedColorInfo = style.SelectedColor(this);
                    if (selectedColorInfo != null)
                        _selectedHtmlSpan = string.Format(span, selectedColorInfo.Hex, "{0}");

                    SetSpannedText(_textHtmlSpan);
                    break;
                default:
                    throw new NotImplementedException("Text format not found: " + _textFormat);
            }

            // selected-background
            UIColor selectedBackground = style.SelectedBackground(this).ToNullableColor();
            if (selectedBackground != null)
            {
                if (backgroundImage != null)
                {
                    using (UIImage selectedImage = GetFilteredImage(backgroundImage, selectedBackground))
                        _view.SetBackgroundImage(selectedImage, UIControlState.Highlighted);
                    selectedBackground.Dispose();
                }
                else
                    _selectedBackground = selectedBackground;
            }

            // text align
            SetTextAlign(style.TextAlign(this, TextAlignValues.Center), nowrap);
            {

                // text padding
                float pl = style.PaddingLeft(this, styleBound.Width);
                float pt = style.PaddingTop(this, styleBound.Height);
                float pr = style.PaddingRight(this, styleBound.Width);
                float pb = style.PaddingBottom(this, styleBound.Height);
                _view.ContentEdgeInsets = new UIEdgeInsets(pt, pl, pb, pr);

                // resize by background image
                IBound bound = GetBoundByImage(styleBound, maxBound, backgroundImage);

                // size to content
                bound = MergeBoundByContent(style, bound, maxBound, _view.ContentEdgeInsets, backgroundImage != null);

                _view.TitleLabel.PreferredMaxLayoutWidth = bound.Width - pl - pr;

                return bound;
            }
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            // we forbid execution of SetSpannedString() 
            _blinkAllowed = false;

            IStyleHelper helper = StyleSheetContext.Current.CreateHelper(styles, CurrentStyleSheet, this);

            UIImage backgroundImage = _view.CurrentBackgroundImage;

            if (styles.Count > 0)
            {
                // background color, background image, borders
                UIImage image = helper.SetBackgroundSettings(this);
                if (image != null && image != backgroundImage)
                {
                    backgroundImage.Dispose();
                    backgroundImage = image;
                    _view.SetBackgroundImage(backgroundImage, UIControlState.Normal);
                }

                // font
                UIFont f;
                if (helper.FontChanged(styleBound.Height, out f))
                    _view.Font = f;

                // word wrap
                IWhiteSpace whiteSpace;
                bool whiteSpaceChanged = helper.TryGet(out whiteSpace);
                bool nowrap = whiteSpace.Kind == WhiteSpaceKind.Nowrap;
                if (whiteSpaceChanged)
                    _view.TitleLabel.LineBreakMode = !nowrap ? UILineBreakMode.WordWrap : UILineBreakMode.TailTruncation;

                // text-format
                ITextFormat textFormat;
                bool formatChanged = helper.TryGet(out textFormat);
                _textFormat = textFormat.Format;

                switch (_textFormat)
                {
                    case TextFormatValues.Text:
                        {
                            _view.SetAttributedTitle(new NSAttributedString(), UIControlState.Normal);
                            SetText();

                            // text color
                            ITextColor textColor;
                            if (helper.TryGet(out textColor) || formatChanged)
                            {
                                _textColor = textColor.ToColorOrClear();
                                _view.SetTitleColor(_textColor, UIControlState.Normal);
                            }

                            // selected-color
                            ISelectedColor selectedColor;
                            if (helper.TryGet(out selectedColor) || formatChanged)
                                _view.SetTitleColor(selectedColor.ToNullableColor() ?? _textColor,
                                    UIControlState.Highlighted);
                        }
                        break;
                    case TextFormatValues.Html:
                        {
                            string whitespace = nowrap ? "nowrap" : "normal";
                            string span = string.Format(
                                "<span style=\"font-family: {0}; font-size: {1:F0}; color: {2}; white-space: {3}\">{4}</span>"
                                , _view.Font.FamilyName, _view.Font.PointSize, "{0}", whitespace, "{1}");
                            // text color
                            ITextColor textColor;
                            formatChanged |= helper.TryGet(out textColor);
                            _textHtmlSpan = string.Format(span, textColor.Value.Hex, "{0}");

                            // selected-color
                            ISelectedColor selectedColor;
                            formatChanged |= helper.TryGet(out selectedColor);
                            if (selectedColor.ToNullableColor() != null)
                                _selectedHtmlSpan = string.Format(span, selectedColor.Value.Hex, "{0}");

                            if (formatChanged)
                                SetSpannedText(_textHtmlSpan);
                        }
                        break;
                    default:
                        throw new NotImplementedException("Text format not found: " + _textFormat);
                }

                // selected-background
                ISelectedBackground selectedBackground;
                if (helper.TryGet(out selectedBackground))
                {
                    UIColor color = selectedBackground.ToNullableColor();
                    if (backgroundImage != null)
                    {
                        if (color != null)
                        {
                            using (UIImage selectedImage = GetFilteredImage(backgroundImage, color))
                                _view.SetBackgroundImage(selectedImage, UIControlState.Highlighted);
                            color.Dispose();
                        }
                        else
                            _view.SetBackgroundImage(backgroundImage, UIControlState.Highlighted);
                    }
                    else
                        _selectedBackground = color;
                }

                // text align
                ITextAlign textAlign;
                if (helper.TryGet(out textAlign))
                    SetTextAlign(textAlign.Align, nowrap);

                // text padding
                float screenWidth = ApplicationContext.Current.DisplayProvider.Width;
                float screenHeight = ApplicationContext.Current.DisplayProvider.Height;
                float pl = helper.Get<IPaddingLeft>().CalcSize(styleBound.Width, screenWidth);
                float pt = helper.Get<IPaddingTop>().CalcSize(styleBound.Height, screenHeight);
                float pr = helper.Get<IPaddingRight>().CalcSize(styleBound.Width, screenWidth);
                float pb = helper.Get<IPaddingBottom>().CalcSize(styleBound.Height, screenHeight);
                _view.ContentEdgeInsets = new UIEdgeInsets(pt, pl, pb, pr);
            }

            // resize by background image
            IBound bound = GetBoundByImage(styleBound, maxBound, backgroundImage);
            _blinkAllowed = true;

            // size to content
            return MergeBoundByContent(CurrentStyleSheet.Helper, bound, maxBound, _view.ContentEdgeInsets,
                backgroundImage != null);
        }

        protected override void Dismiss()
        {
            DisposeField(ref _mainBackground);
            DisposeField(ref _selectedBackground);
            DisposeField(ref _textColor);

            _view.TouchUpInside -= HandleTouchUpInside;
            _view.TouchUpOutside -= HandleTouchUpOutside;
            _view.TouchDown -= HandleTouchDown;  
        }

        protected virtual bool InvokeClick()
        {
            if (OnClick != null || OnClickAction != null)
            {                
                CloseModalWindows();
                EndEditing();
            }

            bool allowed = true;
            if (!string.IsNullOrWhiteSpace(SubmitScope))
                allowed = CurrentContext.Validate(SubmitScope);

            if (allowed)
            {
                if (OnClick != null)
                {
                    LogManager.Logger.Clicked(Id, OnClick.Expression, Text);
                    CurrentContext.JokeProviderInternal.OnTap();
                    OnClick.Execute();
                    return true;
                }

                if (OnClickAction != null)
                {
                    LogManager.Logger.Clicked(Id, OnClickAction.Expression, Text);
                    CurrentContext.JokeProviderInternal.OnTap();
                    OnClickAction.Execute();
                    return true;
                }
            }
            return false;
        }

        private void SetText()
        {
            if (_view != null)
            {
                switch (_textFormat)
                {
                    case TextFormatValues.Text:
                        _view.SetTitle(_text, UIControlState.Normal);
                        break;
                    case TextFormatValues.Html:
                        SetSpannedText(_textHtmlSpan);
                        break;
                    default:
                        throw new NotImplementedException("Text format not found: " + _textFormat);
                }
            }
        }

        private void SetSpannedText(string span, bool force = true)
        {
            if (_view != null && span != null)
            {
                UIControlContentHorizontalAlignment alignment = _view.HorizontalAlignment;
                UIEdgeInsets textInset = _view.ContentEdgeInsets;

                var str = new NSString(string.Format(span, _text));
                var error = new NSError();

                var title = new NSAttributedString(str.DataUsingEncoding(NSStringEncoding.Unicode)
                    , new NSAttributedStringDocumentAttributes { DocumentType = NSDocumentType.HTML }
                    , ref error);

                if (_view != null && (force || _blinkAllowed))
                {
                    _view.SetAttributedTitle(title, UIControlState.Normal);
                    _view.HorizontalAlignment = alignment;
                    _view.ContentEdgeInsets = textInset;
                }
            }
        }

        private void HandleTouchUpInside(object sender, EventArgs e)
        {
            AnimateUp();

            InvokeClick();
        }

        private void HandleTouchUpOutside(object sender, EventArgs e)
        {
            AnimateUp();
        }

        private void HandleTouchDown(object sender, EventArgs e)
        {
            AnimateDown();
        }

        private void AnimateDown()
        {
            if (_view != null)
            {
                if (_selectedBackground != null)
                {
                    _mainBackground = _view.BackgroundColor;
                    _view.BackgroundColor = _selectedBackground;
                }

                if (_textFormat == TextFormatValues.Html) // todo: fix this dirty hack
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        CurrentContext.InvokeOnMainThread(() => SetSpannedText(_selectedHtmlSpan, false));
                        Thread.Sleep(300);
                        CurrentContext.InvokeOnMainThread(() => SetSpannedText(_textHtmlSpan, false));
                    });
            }
        }

        private void AnimateUp()
        {
            if (_view != null)
            {
                if (_selectedBackground != null)
                {
                    _view.BackgroundColor = _mainBackground;
                    _mainBackground.Dispose();
                    _mainBackground = null;
                }
            }
        }


        private void SetTextAlign(TextAlignValues align, bool nowrap)
        {
            switch (align)
            {
                case TextAlignValues.Left:
                    _view.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
                    break;
                case TextAlignValues.Center:
                    _view.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
                    break;
                case TextAlignValues.Right:
                    if (nowrap)
                        _view.TitleLabel.LineBreakMode = UILineBreakMode.HeadTruncation;

                    _view.HorizontalAlignment = UIControlContentHorizontalAlignment.Right;
                    break;
            }
        }
    }
}