using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MonoTouch.UIKit;
using BitMobile.UI;
using BitMobile.IOS;
using BitMobile.Controls.StyleSheet;
using MonoTouch.Foundation;

namespace BitMobile.Controls
{
	public partial class TextView : Control<UITextView>
	{
		string _text = "";
		UIColor _textColor;
		UIColor _selectedColor;
		string _textHtmlSpan;
		string _selectedHtmlSpan;
		TextFormat.Format _textFormat;

		public TextView ()
		{
		}

		public String Text {
			get {
				return _text;
			}
			set {
				_text = value;

				SetText ();
			}
		}

		public override UIView CreateView ()
		{
			_view = new UITextView ();
			_view.Editable = false;
			_view.Selectable = false;
			_view.UserInteractionEnabled = false;
			_view.TextContainer.LineFragmentPadding = 0;

			return _view;
		}

		public override Bound Apply (StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
		{
			base.Apply (stylesheet, styleBound, maxBound);

			StyleHelper style = stylesheet.GetHelper<StyleHelper> ();

			_view.Text = _text;

			// background color, borders
			style.SetBackgroundSettings (this);

			//  text-format
			_textFormat = style.TextFormat (this);

			// font
			UIFont f = style.Font (this, styleBound.Height);
			if (f != null)
				_view.Font = f;

			switch (_textFormat) {
			case TextFormat.Format.Text:
				_view.Text = _text;

				// text color
				_view.TextColor = _textColor = style.ColorOrClear<BitMobile.Controls.StyleSheet.Color> (this);

				// selected-color
				_selectedColor = style.Color<SelectedColor> (this);

				break;
			case TextFormat.Format.Html:
				string span = string.Format ("<span style=\"font-family: {0}; font-size: {1:F0}; color: {2}\">{3}</span>", _view.Font.FamilyName, _view.Font.PointSize, "{0}", "{1}");

				// text color
				_textHtmlSpan = string.Format (span, style.ColorString<BitMobile.Controls.StyleSheet.Color> (this), "{0}");

				// selected-color
				string selectedColor = style.ColorString<SelectedColor> (this);
				if (selectedColor != null)
					_selectedHtmlSpan = string.Format (span, selectedColor, "{0}");

				SetSpannedText (_textHtmlSpan);
				break;
			default:
				throw new NotImplementedException ("Text format not found: " + _textFormat);
			}

			// word wrap
			bool nowrap = style.WhiteSpaceKind (this) == WhiteSpace.Kind.Nowrap;
			if (!nowrap) {	
				_view.TextContainer.LineBreakMode = UILineBreakMode.WordWrap;
				_view.TextContainer.MaximumNumberOfLines = 0;
			} else {
				_view.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
				_view.TextContainer.MaximumNumberOfLines = 1;
			}

			// text align
			switch (style.TextAlign (this)) {
			case TextAlign.Align.Left:
				_view.TextAlignment = UITextAlignment.Left;
				break;
			case TextAlign.Align.Center:
				_view.TextAlignment = UITextAlignment.Center;
				break;
			case TextAlign.Align.Right:
				if (nowrap) {
					_view.TextContainer.LineBreakMode = UILineBreakMode.HeadTruncation;
				}
				_view.TextAlignment = UITextAlignment.Right;
				break;
			}

			// text padding
			float pl = style.Padding<PaddingLeft> (this, styleBound.Width);
			float pt = style.Padding<PaddingTop> (this, styleBound.Height);
			float pr = style.Padding<PaddingRight> (this, styleBound.Width);
			float pb = style.Padding<PaddingBottom> (this, styleBound.Height);
			_view.TextContainerInset = new UIEdgeInsets (pt, pl, pb, pr);

			Bound resultBound = styleBound;

			// size to content
			bool sizeByWidth = style.SizeToContentWidth (this);
			bool sizeByHeight = style.SizeToContentHeight (this);
			if (sizeByWidth || sizeByHeight) {

				float measureWidth = sizeByWidth ? maxBound.Width : styleBound.Width;
				float measureHeight = sizeByHeight ? maxBound.Height : styleBound.Height;

				SizeF size = _view.SizeThatFits (new SizeF (measureWidth, measureHeight));

				float w = sizeByWidth ? size.Width + pl + pr : styleBound.Width;
				float h = sizeByHeight ? size.Height + pt + pb : styleBound.Height;

				resultBound = new Bound (w, h);
			}

			return resultBound;
		}

		public override void AnimateTouch (TouchEventType touch)
		{
			Console.WriteLine (touch);
			switch (_textFormat) {
			case TextFormat.Format.Text:
				if (_selectedColor != null)
					switch (touch) {
					case TouchEventType.Begin:
						_view.TextColor = _selectedColor;
						break;
					case TouchEventType.Cancel:
					case TouchEventType.End:
						UIView.BeginAnimations (null);
						UIView.SetAnimationDuration (0.1);
						_view.TextColor = _textColor;
						UIView.CommitAnimations ();
						break;
					}
				break;
			case TextFormat.Format.Html:
				if (_selectedHtmlSpan != null) {
					switch (touch) {
					case TouchEventType.Begin:
						SetSpannedText (_selectedHtmlSpan);
						break;
					case TouchEventType.Cancel:
					case TouchEventType.End:
						UIView.BeginAnimations (null);
						UIView.SetAnimationDuration (0.1);
						SetSpannedText (_textHtmlSpan);
						UIView.CommitAnimations ();
						break;
					}
				}
				break;
			default:
				throw new NotImplementedException ("Text format not found: " + _textFormat);
			}
		}

		void SetSpannedText(string span)
		{
			NSString str = new NSString (string.Format (span, _text));
			NSError error = new NSError ();
			_view.AttributedText = new NSAttributedString (str.DataUsingEncoding (NSStringEncoding.Unicode)
				, new NSAttributedStringDocumentAttributes () { DocumentType = NSDocumentType.HTML }
				, ref error);

			Console.WriteLine (error);
		}

		void SetText ()
		{
			if (_view != null) {
				switch (_textFormat) {
				case TextFormat.Format.Text:
					_view.Text = _text;
					break;
				case TextFormat.Format.Html:
					var alignment = _view.TextAlignment;
					var textInset = _view.TextContainerInset;
					SetSpannedText (_textHtmlSpan);
					_view.TextAlignment = alignment;
					_view.TextContainerInset = textInset;
					break;
				default:
					throw new NotImplementedException ("Text format not found: " + _textFormat);
				}
			}
		}

		public class NativeLabel: UILabel
		{
			public	float PaddingTop { get; set; }

			public	float PaddingLeft { get; set; }

			public	float PaddingBottom { get; set; }

			public	float PaddingRight { get; set; }

			public override void DrawText (RectangleF rect)
			{
				RectangleF newRect = new RectangleF (rect.X + PaddingLeft, rect.Y + PaddingTop, rect.Width - PaddingLeft - PaddingRight, rect.Height - PaddingTop - PaddingBottom);
				base.DrawText (newRect);
			}
		}
	}
}
