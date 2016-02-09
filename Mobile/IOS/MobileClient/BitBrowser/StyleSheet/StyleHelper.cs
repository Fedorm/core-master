using System;
using BitMobile.Controls.StyleSheet;
using BitMobile.Controls;
using System.Collections.Generic;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
	public class StyleHelper: IStyleSheetHelper
	{
		IOSStyleSheet _stylesheet;
		// TODO: add image cache
		public StyleHelper (IOSStyleSheet stylesheet)
		{
			_stylesheet = stylesheet;
		}

		#region Size

		float Size<T> (IStyledObject control, float parentSize, float displayMetric) where T:Size
		{
			float result = 0;

			Dictionary<Type,Style> styles = _stylesheet.GetStyles (control);

			if (styles.ContainsKey (typeof(T))) {
				Size size = (Size)styles [typeof(T)];
				if (size.Measure == Measure.Pixels)
					result = size.Value / UIScreen.MainScreen.Scale;
				else if (size.Measure == Measure.Percent)
					result = size.Value * parentSize / 100;
				else if (size.Measure == Measure.ScreenPercent)
					result = size.Value * displayMetric / 100;
				else if (size.Measure == Measure.Millimetre) {
					float pixelPerMM = JMABarcodeMT.DeviceHardware.Dpi / 25.4f;
					result = size.Value * pixelPerMM / UIScreen.MainScreen.Scale;
				} else
					result = size.Value;
			} 

			return result;
		}

		bool SizeToContent<T>(IStyledObject control)
		{
			bool result = false;

			Dictionary<Type, Style> styles= _stylesheet.GetStyles (control);
			if (styles.ContainsKey(typeof(T))) {
				var size = (Size)styles [typeof(T)];
				result = size.SizeToContent;
			}

			return result;
		}

		public float Width (IStyledObject control, float parentSize)
		{
			return Size<Width> (control, parentSize, UIScreen.MainScreen.ApplicationFrame.Width);
		}

		public float Height (IStyledObject control, float parentSize)
		{
			return Size<Height> (control, parentSize, UIScreen.MainScreen.ApplicationFrame.Height);
		}

		public float Margin<T> (IStyledObject control, float parentSize) where T:Margin
		{
			float displayMetric;
			if (typeof(T) == typeof(MarginLeft) || typeof(T) == typeof(MarginRight))
				displayMetric = UIScreen.Screens [0].ApplicationFrame.Width;
			else if (typeof(T) == typeof(MarginTop) || typeof(T) == typeof(MarginBottom))
				displayMetric = UIScreen.Screens [0].ApplicationFrame.Height;
			else
				throw new ArgumentException ("Incorrect generic type: " + typeof(T).ToString ());

			return Size<T> (control, parentSize, displayMetric);
		}

		public float Padding<T> (IStyledObject control, float parentSize) where T:Padding
		{
			float displayMetric;
			if (typeof(T) == typeof(PaddingLeft) || typeof(T) == typeof(PaddingRight))
				displayMetric = UIScreen.Screens [0].ApplicationFrame.Width;
			else if (typeof(T) == typeof(PaddingTop) || typeof(T) == typeof(PaddingBottom))
				displayMetric = UIScreen.Screens [0].ApplicationFrame.Height;
			else
				throw new ArgumentException ("Incorrect generic type: " + typeof(T).ToString ());

			return Size<T> (control, parentSize, displayMetric);
		}

		public bool HasBorder (IStyledObject control)
		{
			Dictionary<Type,Style> styles = _stylesheet.GetStyles (control);

			if (styles.ContainsKey (typeof(Border))) {
				Border border = (Border)styles [typeof(Border)];
				if (border.Value == "solid")
					return true;
			}
			return false;
		}

		public float BorderWidth (IStyledObject control)
		{
			return Size<BorderWidth> (control, 0, 0);
		}

		public float BorderRadius (IStyledObject control)
		{
			return Size<BorderRadius> (control, 0, 0);
		}

		public DockAlign.Align DockAlign (IStyledObject control)
		{
			Dictionary<Type, Style> styles = _stylesheet.GetStyles (control);

			if (styles.ContainsKey (typeof(DockAlign))) {
				DockAlign align = (DockAlign)styles [typeof(DockAlign)];
				return align.Value;
			} else
				return BitMobile.Controls.StyleSheet.DockAlign.Align.Top;
		}

		public HorizontalAlign.Align HorizontalAlign (IStyledObject control)
		{
			Dictionary<Type, Style> styles = _stylesheet.GetStyles (control);

			if (styles.ContainsKey (typeof(HorizontalAlign))) {
				HorizontalAlign align = (HorizontalAlign)styles [typeof(HorizontalAlign)];
				return align.Value;
			} else
				return BitMobile.Controls.StyleSheet.HorizontalAlign.Align.Left;
		}

		public VerticalAlign.Align VerticalAlign (IStyledObject control)
		{
			Dictionary<Type, Style> styles = _stylesheet.GetStyles (control);

			if (styles.ContainsKey (typeof(VerticalAlign))) {
				VerticalAlign align = (VerticalAlign)styles [typeof(VerticalAlign)];
				return align.Value;
			} else
				return BitMobile.Controls.StyleSheet.VerticalAlign.Align.Top;
		}

		public bool SizeToContentWidth (IStyledObject control)
		{
			return SizeToContent<Width> (control);		
		}

		public bool SizeToContentHeight (IStyledObject control)
		{
			return SizeToContent<Height> (control);
		}

		#endregion

		#region Drawable

		public UIColor ColorOrClear<T> (IControl<UIView> control) where T:Color
		{
			return Color<T> (control) ?? UIColor.Clear;
		}

		public UIColor Color<T> (IControl<UIView> control) where T:Color
		{
			UIColor result = null;

			Dictionary<Type,Style> styles = _stylesheet.GetStyles (control);

			if (styles.ContainsKey (typeof(T))) {
				Color color = (Color)styles [typeof(T)];
				result = FromHexString (color.Value);
			}

			return result;
		}

		public string ColorString<T> (IControl<UIView> control) where T:Color
		{
			Dictionary<Type,Style> styles = _stylesheet.GetStyles (control);

			if (styles.ContainsKey (typeof(T))) {
				Color color = (Color)styles [typeof(T)];
				return color.Value;
			}

			return null;
		}

		public String BackgroundImage (IControl<UIView> control)
		{
			Dictionary<Type,Style> styles = _stylesheet.GetStyles (control);

			if (styles.ContainsKey (typeof(BackgroundImage))) {
				BackgroundImage img = (BackgroundImage)styles [typeof(BackgroundImage)];
				return img.Path;
			} else
				return null;
		}

		public void SetBackgroundSettings (IControl<UIView> control)
		{
			control.View.BackgroundColor = ColorOrClear<BackgroundColor> (control);

			if (HasBorder (control)) {
                control.View.ClipsToBounds = true;
				control.View.Layer.BorderWidth = BorderWidth (control);
				control.View.Layer.BorderColor = ColorOrClear<BorderColor> (control).CGColor;
				control.View.Layer.CornerRadius = BorderRadius (control);
			}
		}

		static UIColor FromHexString (string hexValue, float alpha = 1.0f)
		{
			if (hexValue.ToLower ().Equals ("none"))
				return null;

			var colorString = hexValue.Replace ("#", "");
			if (alpha > 1.0f) {
				alpha = 1.0f;
			} else if (alpha < 0.0f) {
				alpha = 0.0f;
			}

			float red, green, blue;

			switch (colorString.Length) {
			case 3: // #RGB
				{
					red = Convert.ToInt32 (string.Format ("{0}{0}", colorString.Substring (0, 1)), 16) / 255f;
					green = Convert.ToInt32 (string.Format ("{0}{0}", colorString.Substring (1, 1)), 16) / 255f;
					blue = Convert.ToInt32 (string.Format ("{0}{0}", colorString.Substring (2, 1)), 16) / 255f;
					return UIColor.FromRGBA (red, green, blue, alpha);
				}
			case 6: // #RRGGBB
				{
					red = Convert.ToInt32 (colorString.Substring (0, 2), 16) / 255f;
					green = Convert.ToInt32 (colorString.Substring (2, 2), 16) / 255f;
					blue = Convert.ToInt32 (colorString.Substring (4, 2), 16) / 255f;
					return UIColor.FromRGBA (red, green, blue, alpha);
				}   

			default :
				throw new ArgumentOutOfRangeException (string.Format ("Invalid color value {0} is invalid. It should be a hex value of the form #RBG, #RRGGBB", hexValue));

			}
		}

		#endregion

		#region Font

		public UIFont Font (IControl<UIView> control, float height)
		{
			Dictionary<Type,Style> styles = _stylesheet.GetStyles (control);

			if (styles.ContainsKey (typeof(Font))) {
				Font font = (Font)styles [typeof(Font)];
				float size = font.Size;

				if (font.Measure == Measure.Pixels)
					size = font.Size / UIScreen.MainScreen.Scale;
				else if (font.Measure == Measure.Percent)
					size = font.Size * height / 100;
				else if (font.Measure == Measure.ScreenPercent)
					size = font.Size * UIScreen.MainScreen.ApplicationFrame.Height / 100;
				else if (font.Measure == Measure.Millimetre) {
					float pixelPerMM = JMABarcodeMT.DeviceHardware.Dpi / 25.4f;
					size = font.Size * pixelPerMM / UIScreen.MainScreen.Scale;
				} else
					size = font.Size;

				return UIFont.FromName (font.Name, size);
			} else
				return null;
		}

		public TextAlign.Align TextAlign (IControl<UIView> control)
		{
			Dictionary<Type,Style> styles = _stylesheet.GetStyles (control);

			if (styles.ContainsKey (typeof(TextAlign))) {
				TextAlign align = (TextAlign)styles [typeof(TextAlign)];
				return align.Value;
			} else
				return BitMobile.Controls.StyleSheet.TextAlign.Align.Left;
		}

		public TextFormat.Format TextFormat (IControl<UIView> control)
		{
			Dictionary<Type,Style> styles = _stylesheet.GetStyles (control);

			if (styles.ContainsKey (typeof(TextFormat))) {
				TextFormat format = (TextFormat)styles [typeof(TextFormat)];
				return format.Value;
			} else
				return BitMobile.Controls.StyleSheet.TextFormat.Format.Text;
		}

		public  WhiteSpace.Kind WhiteSpaceKind (IControl<UIView> control)
		{
			Dictionary<Type, Style> styles = _stylesheet.GetStyles (control);

			if (styles.ContainsKey (typeof(WhiteSpace))) {
				WhiteSpace wsk = (WhiteSpace)styles [typeof(WhiteSpace)];
				return wsk.Value;
			} else
				return WhiteSpace.Kind.Nowrap;
		}

		#endregion
	}
}

