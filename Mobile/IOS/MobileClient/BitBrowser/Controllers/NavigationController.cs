using System;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
	public class NavigationController : UINavigationController
	{
		public NavigationController ()
		{
			this.ToolbarHidden = true;
			this.NavigationBarHidden = true;
		}
	}
}

