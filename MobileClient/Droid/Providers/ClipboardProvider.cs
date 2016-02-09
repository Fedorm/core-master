using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Text;
using BitMobile.Common;
using BitMobile.Common.Device.Providers;
using ClipboardManager = Android.Content.ClipboardManager;

namespace BitMobile.Droid.Providers
{
    class ClipboardProvider : IClipboardProvider
    {
        private readonly BaseScreen _baseActivity;

        public ClipboardProvider(BaseScreen baseActivity)
        {
            _baseActivity = baseActivity;
        }

        public bool HasStringValue
        {
            get
            {
                using (var manager = GetClipboardManager())
                    return !string.IsNullOrEmpty(manager.Text);
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
                if (!string.IsNullOrEmpty(manager.Text))
                    return manager.Text;

            return null;
        }

        private ClipboardManager GetClipboardManager()
        {
            return (ClipboardManager)_baseActivity.GetSystemService(Context.ClipboardService);
        }
    }
}
