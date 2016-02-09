
using Android.App;
using Android.Content;
using BitMobile.Common.Device.Providers;
using System;

namespace BitMobile.Droid.Providers
{
    class WebProvider : IWebProvider
    {
        private Activity _activity;

        public WebProvider(Activity activity)
        {
            _activity = activity;
        }

        public void OpenUrl(Uri url)
        {
            var uri = Android.Net.Uri.Parse(url.ToString());
            var intent = new Intent(Intent.ActionView, uri);
            _activity.StartActivity(intent);
        }
    }
}