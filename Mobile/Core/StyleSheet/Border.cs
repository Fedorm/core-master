using System;

namespace BitMobile.Controls.StyleSheet
{
	[Synonym("border-style")]
	public class Border : Style
	{
		private String value;
		
		public String Value 
		{
			get 
			{
				return this.value;
			}
			set 
			{
				this.value = value;
			}
		}
		
		public override Style FromString (string s)
		{
			s = s.Trim().ToLower();
			if(!(s.Equals("none") || s.Equals("solid")))
			   throw new Exception("Invalid border value");
			this.value = s;
			return this;
		}
	}

	[Synonym("border-width")]
	public class BorderWidth : Size
	{
	}
}

