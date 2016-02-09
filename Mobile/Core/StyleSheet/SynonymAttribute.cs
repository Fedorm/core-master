using System;

namespace BitMobile.Controls.StyleSheet
{
	public class SynonymAttribute : Attribute
	{
		private String name;

		public String Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		public SynonymAttribute (String name)
		{
			this.name = name;
		}
	}
}

