using System;

namespace BitMobile.Controls.StyleSheet
{
	[Synonym("background-image")]
	public partial class BackgroundImage : Style
	{
		private String path;

		public BackgroundImage()
		{
		}

		public String Path {
			get {
				return path;
			}
		}

		public override Style FromString (string s)
		{
			s = s.Trim();
			System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"url\((?<value>.+)\)");
			System.Text.RegularExpressions.Match match = reg.Match(s);
			if(match.Success)
			{
				path = match.Groups["value"].Value;
			}
			return this;
		}
	}
}
