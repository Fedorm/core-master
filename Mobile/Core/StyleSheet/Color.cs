using System;

namespace BitMobile.Controls.StyleSheet
{
	public class Color : Style
	{
		public Color ()
		{
		}

		private String value;
		
		public String Value {
			get {
				return value;
			}
		}
		
		public override Style FromString (string s)
		{
			value = s;
			return this;
		}
	}

	[Synonym("text-color")]
	public class TextColor : Color
	{
	}

    [Synonym("placeholder-color")]
    public class PlaceholderColor : Color
    {
    }

	[Synonym("background-color")]
	public class BackgroundColor : Color
	{
	}

	[Synonym("selected-color")]
	public class SelectedColor : Color
	{
	}

	[Synonym("border-color")]
	public class BorderColor : Color
	{
	}
}

