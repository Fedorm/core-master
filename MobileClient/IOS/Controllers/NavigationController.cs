using MonoTouch.UIKit;

namespace BitMobile.IOS
{
    public class NavigationController : UINavigationController
    {
        public NavigationController()
        {
            ToolbarHidden = true;
            NavigationBarHidden = true;
        }
    }
}