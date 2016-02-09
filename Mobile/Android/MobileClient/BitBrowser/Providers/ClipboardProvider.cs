using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Text;
using BitMobile.Common;

namespace BitMobile.Droid.Providers
{
    class ClipboardProvider : IClipboardProvider
    {
        private readonly BaseScreen _baseActivity;
        private readonly ApplicationContext _applicationContext;

        public ClipboardProvider(BaseScreen baseActivity, ApplicationContext applicationContext)
        {
            _baseActivity = baseActivity;
            _applicationContext = applicationContext;
        }

        public bool HasStringValue
        {
            get
            {
                using (var manager = GetClipboardManager())
                    return manager.HasText;
            }
        }

        public bool SetString(string str)
        {
            if (str == null)
                return false;

            using (var manager = GetClipboardManager())
                manager.Text = str;

            return true;
        }

        public string GetString()
        {
            using (var manager = GetClipboardManager())
                if (manager.HasText)
                    return manager.Text;

            return null;
        }

        private ClipboardManager GetClipboardManager()
        {
            return (ClipboardManager)_baseActivity.GetSystemService(Context.ClipboardService);
        }
    }
}
