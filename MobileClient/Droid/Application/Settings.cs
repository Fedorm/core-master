using Android.Content;
using BitMobile.Application;
using BitMobile.Common;
using BitMobile.Common.Application;

namespace BitMobile.Droid.Application
{
    public class Settings : ApplicationSettings
    {
        readonly ISharedPreferences _preferences;

        readonly string _name;

        public Settings(ISharedPreferences preferences, string systemLanguage, InfobaseManager.Infobase infobase)
        {
            _preferences = preferences;
            Language = systemLanguage;

            _name = infobase.Name;

            BaseUrl = infobase.BaseUrl;
            ApplicationString = infobase.ApplicationString;

            FtpPort = infobase.FtpPort;

            string lastUrl = _preferences.GetString("url", string.Empty);
            if (BaseUrl == lastUrl)
            {
                UserName = _preferences.GetString("user", infobase.UserName);
                Password = _preferences.GetString("password", infobase.Password);
            }
            else
            {
                UserName = infobase.UserName;
                Password = infobase.Password;
            }

            ClearCacheOnStart = infobase.IsActive && _preferences.GetBoolean("clearCache", ForceClearCache);

            infobase.IsActive = true;
            infobase.IsAutorun = true;

            WriteSettings();
        }

        public override IApplicationSettings ReadSettings()
        {
            BaseUrl = _preferences.GetString("url", DefaultUrl);
            ApplicationString = _preferences.GetString("application", DefaultApplication);

            UserName = _preferences.GetString("user", DefaultUserName);
            Password = _preferences.GetString("password", DefaultPassword);

            ClearCacheOnStart = _preferences.GetBoolean("clearCache", ForceClearCache);

            return this;
        }

        public override sealed void WriteSettings()
        {
            ISharedPreferencesEditor editor = _preferences.Edit();
            editor.PutString("url", BaseUrl);
            editor.PutString("application", ApplicationString);
            editor.PutString("user", UserName);
            editor.PutString("password", Password);
            editor.PutBoolean("clearCache", ClearCacheOnStart);            

            editor.PutString("ftpPort", FtpPort);
            editor.PutString("platformVersion", CoreInformation.CoreVersion.ToString());

            if (ConfigName != null)
                editor.PutString("configuration", ConfigName);
            if (ConfigVersion != null)
                editor.PutString("version", ConfigVersion);

            editor.Commit();

            InfobaseManager.Current.SaveInfobase(_name, this);
        }

        public void SetClearCacheDisabled()
        {
            if (!ForceClearCache)
                if (ClearCacheOnStart)
                {
                    ClearCacheOnStart = false;
                    WriteSettings();
                }
        }

        public void LogOut()
        {
            foreach (var infobase in InfobaseManager.Current.Infobases)
                infobase.IsAutorun = false;
            InfobaseManager.Current.SaveInfobases();
        }
    }
}