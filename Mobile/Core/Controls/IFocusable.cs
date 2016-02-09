using System;

namespace BitMobile.Controls
{
	public interface IFocusable
	{
		bool AutoFocus { get; set; }
		string Keyboard { get; set; }
	}
}

