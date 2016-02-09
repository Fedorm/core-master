using BitMobile.Common.Device.Providers;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System;

namespace BitMobile.IOS.Providers
{
    class WebProvider : IWebProvider
    {
        public void OpenUrl(Uri url)
        {
            UIApplication.SharedApplication.OpenUrl(new NSUrl(url.ToString()));
        }
    }
}
