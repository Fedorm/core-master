using System;
using System.Collections.Generic;
using System.Threading;
using BitMobile.Application;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.IOS;
using BitMobile.UI;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using BitMobile.Application.StyleSheet;

namespace BitMobile.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "TextView")]
    public class TextView : Control<UITextView>
    {
        private UIImage _backgroundImage;
        private bool _blinkAllowed = true;
        private UIColor _mainBackground;
        private CGImage _mainBackgroundImage;
        private UIColor _selectedBackground;
        private CGImage _selectedBackgroundImage;
        private UIColor _selectedColor;
        private string _selectedHtmlSpan;
        private string _text = "";
        private UIColor _textColor;
        private TextFormatValues _textFormat;
        private string _textHtmlSpan;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                SetText();
            }
        }

        public override void CreateView()
        {
            _view = new UITextView();
            _view.Editable = false;
            _view.Selectable = false;
            _view.ScrollEnabled = false;
            _view.UserInteractionEnabled = false;
            _view.TextContainer.LineFragmentPadding = 0;
        }

        public override void AnimateTouch(TouchEventType touch)
        {
            switch (_textFormat)
            {
                case TextFormatValues.Text:
                    if (_selectedColor != null)
                        switch (touch)
                        {
                            case TouchEventType.Begin:
                                BackgroundAnimationDown();
                                _view.TextColor = _selectedColor;
                                break;
                            case TouchEventType.Cancel:
                            case TouchEventType.End:
                                BackgroundAnimationUp();

                                UIView.BeginAnimations(null);
                                UIView.SetAnimationDuration(0.1);

                                _view.TextColor = _textColor;

                                UIView.CommitAnimations();
                                break;
                        }
                    break;
                case TextFormatValues.Html:
                    {
                        switch (touch)
                        {
                            case TouchEventType.Begin:
                                BackgroundAnimationDown();

                                if (_selectedHtmlSpan != null)
                                    ThreadPool.QueueUserWorkItem(state =>
                                    {
                                        CurrentContext.InvokeOnMainThread(
                                            () => SetSpannedText(_selectedHtmlSpan, false));
                                        Thread.Sleep(500);
                                        CurrentContext.InvokeOnMainThread(() => SetSpannedText(_textHtmlSpan));
                                    });

                                break;
                            case TouchEventType.Cancel:
                            case TouchEventType.End:
                                BackgroundAnimationUp();

                                UIView.BeginAnimations(null);
                                UIView.SetAnimationDuration(0.1);

                                if (_selectedHtmlSpan != null)
                                    SetSpannedText(_textHtmlSpan);

                                UIView.CommitAnimations();
                                break;
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException("Text format not found: " + _textFormat);
            }
        }

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            _view.Text = _text;

            IStyleSheetHelper style = stylesheet.Helper;

            // background color, borders, background image
            _backgroundImage = stylesheet.SetBackgroundSettings(this);
            if (_backgroundImage != null)
            {
                _view.Layer.Contents = _backgroundImage.CGImage;
                _view.BackgroundColor = UIColor.Clear;
            }

            //  text-format
            _textFormat = style.TextFormat(this);

            // font
            UIFont f = stylesheet.Font(this, styleBound.Height);
            if (f != null)
                _view.Font = f;

            switch (_textFormat)
            {
                case TextFormatValues.Text:
                    _view.Text = _text;

                    // text color
                    _view.TextColor = _textColor = style.Color(this).ToColorOrClear();

                    // selected-color
                    _selectedColor = style.SelectedColor(this).ToNullableColor();

                    break;
                case TextFormatValues.Html:
                    string span =
                        string.Format("<span style=\"font-family: {0}; font-size: {1:F0}; color: {2}\">{3}</span>",
                            _view.Font.FamilyName, _view.Font.PointSize, "{0}", "{1}");
                    // text color
                    _textHtmlSpan = string.Format(span, style.Color(this).Hex, "{0}");

                    // selected-color
                    IColorInfo selectedColor = style.SelectedColor(this);
                    if (selectedColor != null)
                        _selectedHtmlSpan = string.Format(span, selectedColor.Hex, "{0}");

                    SetSpannedText(_textHtmlSpan);
                    break;
                default:
                    throw new NotImplementedException("Text format not found: " + _textFormat);
            }

            // selected-background
            UIColor selectedBackground = style.SelectedBackground(this).ToNullableColor();
            if (selectedBackground != null)
            {
                if (_backgroundImage != null)
                {
                    _selectedBackgroundImage = GetFilteredImage(_backgroundImage, selectedBackground).CGImage;
                    selectedBackground.Dispose();
                }
                else
                    _selectedBackground = selectedBackground;
            }

            // word wrap
            bool nowrap = style.WhiteSpace(this) == WhiteSpaceKind.Nowrap;
            if (!nowrap)
            {
                _view.TextContainer.LineBreakMode = UILineBreakMode.WordWrap;
                _view.TextContainer.MaximumNumberOfLines = 0;
            }
            else
            {
                _view.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
                _view.TextContainer.MaximumNumberOfLines = 1;
            }

            // text align
            SetTextAlign(style.TextAlign(this), nowrap);

            // text padding
            float pl = style.PaddingLeft(this, styleBound.Width);
            float pt = style.PaddingTop(this, styleBound.Height);
            float pr = style.PaddingRight(this, styleBound.Width);
            float pb = style.PaddingBottom(this, styleBound.Height);
            _view.TextContainerInset = new UIEdgeInsets(pt, pl, pb, pr);

            // resize by background image
            IBound bound = GetBoundByImage(styleBound, maxBound, _backgroundImage);

            // size to content
            return MergeBoundByContent(style, bound, maxBound, _view.TextContainerInset, _backgroundImage != null);
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            // we forbid execution of SetSpannedString() 
            _blinkAllowed = false;

            IStyleHelper helper = StyleSheetContext.Current.CreateHelper(styles, CurrentStyleSheet, this);

            if (styles.Count > 0)
            {
                _view.Text = _text;

                // background color, borders, background image
                UIImage image = helper.SetBackgroundSettings(this);
                if (image != null && image != _backgroundImage)
                {
                    _backgroundImage.Dispose();
                    _backgroundImage = image;
                    _view.Layer.Contents = _backgroundImage.CGImage;
                    _view.BackgroundColor = UIColor.Clear;
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
                    if (!nowrap)
                    {
                        _view.TextContainer.LineBreakMode = UILineBreakMode.WordWrap;
                        _view.TextContainer.MaximumNumberOfLines = 0;
                    }
                    else
                    {
                        _view.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
                        _view.TextContainer.MaximumNumberOfLines = 1;
                    }

                // text-format
                ITextFormat textFormat;
                bool formatChanged = helper.TryGet(out textFormat);
                _textFormat = textFormat.Format;

                switch (_textFormat)
                {
                    case TextFormatValues.Text:
                        {
                            _view.AttributedText = new NSAttributedString();
                            _view.Text = _text;

                            // text color
                            ITextColor textColor;
                            if (helper.TryGet(out textColor) || formatChanged)
                                _view.TextColor = _textColor = textColor.ToColorOrClear();

                            // selected-color
                            ISelectedColor selectedColor;
                            if (helper.TryGet(out selectedColor) || formatChanged)
                                _selectedColor = selectedColor.ToNullableColor();
                        }
                        break;
                    case TextFormatValues.Html:
                        {
                            string span =
                                string.Format("<span style=\"font-family: {0}; font-size: {1:F0}; color: {2}\">{3}</span>",
                                    _view.Font.FamilyName, _view.Font.PointSize, "{0}", "{1}");

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
                    if (_backgroundImage != null && color != null)
                    {
                        _selectedBackgroundImage = GetFilteredImage(_backgroundImage, color).CGImage;
                        color.Dispose();
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
                _view.TextContainerInset = new UIEdgeInsets(pt, pl, pb, pr);
            }
            _blinkAllowed = true;

            // resize by background image
            IBound bound = GetBoundByImage(styleBound, maxBound, _backgroundImage);

            // size to content
            return MergeBoundByContent(CurrentStyleSheet.Helper, bound, maxBound, _view.TextContainerInset,
                _backgroundImage != null);
        }

        protected override void Dismiss()
        {
            DisposeField(ref _backgroundImage);
            DisposeField(ref _mainBackground);
            DisposeField(ref _mainBackgroundImage);
            DisposeField(ref _selectedBackground);
            DisposeField(ref _selectedBackgroundImage);
            DisposeField(ref _selectedColor);
            DisposeField(ref _textColor);
        }

        private void SetTextAlign(TextAlignValues align, bool nowrap)
        {
            switch (align)
            {
                case TextAlignValues.Left:
                    _view.TextAlignment = UITextAlignment.Left;
                    break;
                case TextAlignValues.Center:
                    _view.TextAlignment = UITextAlignment.Center;
                    break;
                case TextAlignValues.Right:
                    if (nowrap)
                        _view.TextContainer.LineBreakMode = UILineBreakMode.HeadTruncation;
                    _view.TextAlignment = UITextAlignment.Right;
                    break;
            }
        }

        private void SetSpannedText(string span, bool force = true)
        {
            if (_view != null && span != null)
            {
                UITextAlignment alignment = _view.TextAlignment;
                UIEdgeInsets textInset = _view.TextContainerInset;

                var str = new NSString(string.Format(span, _text));
                var error = new NSError();

                if (_view != null && (force || _blinkAllowed))
                {
                    _view.AttributedText = new NSAttributedString(str.DataUsingEncoding(NSStringEncoding.Unicode)
                        , new NSAttributedStringDocumentAttributes { DocumentType = NSDocumentType.HTML }
                        , ref error);

                    _view.TextAlignment = alignment;
                    _view.TextContainerInset = textInset;
                }
            }
        }

        private void SetText()
        {
            if (_view != null)
            {
                switch (_textFormat)
                {
                    case TextFormatValues.Text:
                        _view.Text = _text;
                        break;
                    case TextFormatValues.Html:
                        SetSpannedText(_textHtmlSpan);
                        break;
                    default:
                        throw new NotImplementedException("Text format not found: " + _textFormat);
                }
            }
        }

        private void BackgroundAnimationDown()
        {
            if (_selectedBackground != null)
            {
                _mainBackground = _view.BackgroundColor;
                _view.BackgroundColor = _selectedBackground;
            }
            else if (_selectedBackgroundImage != null)
            {
                _mainBackgroundImage = _view.Layer.Contents;
                _view.Layer.Contents = _selectedBackgroundImage;
            }
        }

        private void BackgroundAnimationUp()
        {
            if (_mainBackground != null)
            {
                _view.BackgroundColor = _mainBackground;
                _mainBackground.Dispose();
                _mainBackground = null;
            }
            else if (_mainBackgroundImage != null)
            {
                _view.Layer.Contents = _mainBackgroundImage;
                _mainBackgroundImage.Dispose();
                _mainBackgroundImage = null;
            }
        }
    }
}