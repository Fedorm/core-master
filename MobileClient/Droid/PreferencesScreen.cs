using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Views;
using Android.Widget;
using BitMobile.Application.Translator;
using System.Collections.Generic;
using BitMobile.Common;
using BitMobile.Droid.Application;

namespace BitMobile.Droid
{
    [Activity(Label = "Preferences")]
    class PreferencesScreen : PreferenceActivity, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        private bool _inForeground;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            #region Set default android settings

            RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetSoftInputMode(SoftInput.AdjustPan);
            #endregion

            base.OnCreate(savedInstanceState);

            AddPreferencesFromResource(Resource.Xml.Preferences);

            PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            if (key != "clearCache" && key != "anonymousAccess")
            {
                string value = sharedPreferences.GetString(key, string.Empty);

                FillSummary(key, value);

                if ((key == "user" || key == "password" || key == "url") && !sharedPreferences.GetBoolean("clearCache", true))
                {
                    ISharedPreferencesEditor editor = sharedPreferences.Edit();
                    editor.PutBoolean("clearCache", true);
                    editor.Commit();

                    MakeToast(D.NEED_TO_REBOOT_FULL);
                }

                if (key == "application")
                    MakeToast(D.NEED_TO_REBOOT);
            }
            else
            {
                MakeToast(D.NEED_TO_REBOOT_FULL);
            }

            BitBrowserApp.Current.SyncSettings();
        }

        protected override void OnResume()
        {
            base.OnResume();

            _inForeground = true;

            foreach (KeyValuePair<string, object> item in PreferenceManager.SharedPreferences.All)
            {
                string value = item.Value != null ? item.Value.ToString() : string.Empty;

                FillName(item.Key);

                FillSummary(item.Key, value);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            _inForeground = false;
        }

        void FillSummary(string key, string value)
        {
            Preference preference = FindPreference(key);
            if (preference != null)
            {
                if (key == "configuration")
                {
                    if (BitBrowserApp.Current.AppContext.Dal != null)
                        preference.Summary = BitBrowserApp.Current.AppContext.Dal.ConfigName;

                    preference.Selectable = false;
                }
                else if (key == "version")
                {
                    if (BitBrowserApp.Current.AppContext.Dal != null)
                        preference.Summary = BitBrowserApp.Current.AppContext.Dal.ConfigVersion;
                    preference.Selectable = false;
                }
                else if (key == "ftpPort")
                {
                    preference.Summary = value;
                    preference.Selectable = false;
                }
                else if (key == "platformVersion")
                {
                    preference.Summary = CoreInformation.CoreVersion.ToString();
                    preference.Selectable = false;
                }
                else if (key == "password")
                {
                    preference.Summary = string.Empty;

                    for (int i = 0; i < value.Length; i++)
                        preference.Summary += "*";
                }
                else if (key == "clearCache")
                {
                    preference.Summary = D.CLEAR_CACHE_SUMMARY;
                }
                else if (key == "anonymousAccess")
                {
                }
                else
                    preference.Summary = value;
            }
        }

        void FillName(string key)
        {
            Preference preference = FindPreference(key);

            switch (key)
            {
                case "clearCache":
                    preference.Title = D.CLEAR_CACHE;
                    break;
                case "url":
                    preference.Title = D.URL;
                    break;
                case "ftpPort":
                    preference.Title = D.FTP_PORT;
                    break;
                case "application":
                    preference.Title = D.APPLICATION;
                    break;
                case "user":
                    preference.Title = D.USER_NAME;
                    break;
                case "password":
                    preference.Title = D.PASSWORD;
                    break;
                case "platformVersion":
                    preference.Title = D.PLATFORM_VERSION;
                    break;
                case "configuration":
                    preference.Title = D.CONFIG;
                    break;
                case "version":
                    preference.Title = D.VERSION;
                    break;
            }
        }

        private void MakeToast(string text)
        {
            if (_inForeground)
                Toast.MakeText(this, text, ToastLength.Long).Show();
        }
    }
}