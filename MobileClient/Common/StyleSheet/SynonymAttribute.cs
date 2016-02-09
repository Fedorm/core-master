using System;

namespace BitMobile.Common.StyleSheet
{
	public class SynonymAttribute : Attribute
	{
	    public string Name { get; private set; }

	    public SynonymAttribute (string name)
		{
			Name = name;
		}
	}
}

