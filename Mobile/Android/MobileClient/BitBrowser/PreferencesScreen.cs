using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Views;
using Android.Widget;
using BitMobile.DataAccessLayer;
using BitMobile.Utilities.Translator;
using System.Collections.Generic;

namespace BitMobile.Droid
{
    [Activity(Label = "Preferences")]
    class PreferencesScreen : PreferenceActivity, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            #region Set default android settings

            this.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            this.Window.SetSoftInputMode(SoftInput.AdjustPan);
            #endregion

            base.OnCreate(savedInstanceState);

            AddPreferencesFromResource(Resource.Xml.Preferences);

            this.PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            string value = string.Empty;

            if (key != "clearCache" && key != "anonymousAccess")
            {
                Preference preference = (Preference)FindPreference(key);

                value = sharedPreferences.GetString(key, string.Empty);

                FillSummary(key, value);

                if ((key == "user" || key == "password" || key == "url") && !sharedPreferences.GetBoolean("clearCache", true))
                {
                    ISharedPreferencesEditor editor = sharedPreferences.Edit();
                    editor.PutBoolean("clearCache", true);
                    editor.Commit();

                    Toast.MakeText(this, D.NEED_TO_REBOOT_FULL, ToastLength.Long).Show();
                }

                if (key == "application")
                {
                    Toast.MakeText(this, D.NEED_TO_REBOOT, ToastLength.Long).Show();
                }
            }
            else
            {
                if (key == "anonymousAccess")
                {
                    string user = "";
                    string password = "";
                    if (sharedPreferences.GetBoolean("anonymousAccess", false))
                    {
                        user = Settings.AnonymousUserName;
                        password = Settings.AnonymousPassword;
                    }
                    ISharedPreferencesEditor editor = sharedPreferences.Edit();
                    editor.PutString("user", user);
                    editor.PutString("password", password);
                    editor.Commit();
                }

                Toast.MakeText(this, D.NEED_TO_REBOOT_FULL, ToastLength.Long).Show();
            }

            BitBrowserApp.Current.SyncSettings(true);
        }

        protected override void OnResume()
        {
            base.OnResume();

            foreach (KeyValuePair<string, object> item in this.PreferenceManager.SharedPreferences.All)
            {
                string value = item.Value != null ? item.Value.ToString() : string.Empty;

                FillName(item.Key, value);

                FillSummary(item.Key, value);
            }
        }

        void FillSummary(string key, string value)
        {
            Preference preference = (Preference)FindPreference(key);
            if (preference != null)
            {
                if (key == "configuration")
                {
                    if (BitBrowserApp.Current.AppContext.DAL != null)
                        preference.Summary = BitBrowserApp.Current.AppContext.DAL.ConfigName;

                    preference.Selectable = false;
                }
                else if (key == "version")
                {
                    if (BitBrowserApp.Current.AppContext.DAL != null)
                        preference.Summary = BitBrowserApp.Current.AppContext.DAL.ConfigVersion;
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

        void FillName(string key, string value)
        {
            Preference preference = (Preference)FindPreference(key);

            switch (key)
            {
                case "clearCache":
                    preference.Title = D.CLEAR_CACHE;
                    break;
                case "anonymousAccess":
                    preference.Title = D.ANONYMOUS_ACCESS;
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

        void preference_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            e.Preference.Summary = e.NewValue.ToString();
        }

    }
}