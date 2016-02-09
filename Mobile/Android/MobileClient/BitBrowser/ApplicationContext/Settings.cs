using Android.Content;
using BitMobile.Application;
using BitMobile.DataAccessLayer;

namespace BitMobile.Droid
{
    public class Settings : ApplicationSettings
    {
#if DEBUG
        public const string DefaultUrl = @"http://192.168.106.109/bitmobile/superagent";
        public const string DefaultApplitation = "app -d -t -s -re:kugushew@gmail.com";
        public const string DefaultUserName = "demosr";
        public const string DefaultPassword = "demosr";
        public const bool DemoEnabled = true;
        private const SolutionType DefaultSolutionType = SolutionType.BitMobile;
#else
        //-------------------------------------------------------------------------------        
        //public const string DefaultUrl = @"http://bitmobile.cloudapp.net/demorup";     
        //public const string DefaultUrl = @"http://bitmobile2.cloudapp.net/demorutest";
	    //public const string DefaultUrl = @"http://192.168.0.2/bitmobile/test";
        //-------------------------------------------------------------------------------                
        //public const SolutionType DefaultSolutionType = SolutionType.BitMobile;
        //public const SolutionType DefaultSolutionType = SolutionType.SuperAgent;
	    //public const SolutionType DefaultSolutionType = SolutionType.SuperService;
        //------------------------------------------------------------------------------- 
        // 1. Выбрать адрес и солюшен
        // 2. Заменить Icon.png
        // 3. Заменить ApplicationName
        public const string DefaultApplitation = "app";
        public const string DefaultUserName = "";
        public const string DefaultPassword = "";
        public const bool DemoEnabled = false;
#endif

        public const string DemoUserName = "demo";
        public const string DemoPassword = "demo";

        public const bool IsAnonymousAccess = false;
        public const string AnonymousUserName = "anonymous";
        public const string AnonymousPassword = "anonymous";

        readonly ISharedPreferences _preferences;

        readonly string _name;

        public Settings(ISharedPreferences preferences, string systemLanguage, InfobaseManager.Infobase infobase)
        {
            CurrentSolutionType = DefaultSolutionType;

            _preferences = preferences;
            Language = systemLanguage;

            _name = infobase.Name;

            AnonymousAccess = infobase.AnonymousAccess;

            BaseUrl = infobase.BaseUrl;
            ApplicationString = infobase.ApplicationString;

            FtpPort = infobase.FtpPort;

            if (AnonymousAccess)
            {
                UserName = AnonymousUserName;
                Password = AnonymousPassword;
            }
            else
            {
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
            }

            ClearCacheOnStart = infobase.IsActive && _preferences.GetBoolean("clearCache", ForceClearCache);

            infobase.IsActive = true;
            infobase.IsAutorun = true;

            WriteSettings();
        }
        
        public override ApplicationSettings ReadSettings()
        {
            AnonymousAccess = _preferences.GetBoolean("anonymousAccess", false);

            BaseUrl = _preferences.GetString("url", DefaultUrl);
            ApplicationString = _preferences.GetString("application", DefaultApplitation);

            if (AnonymousAccess)
            {
                UserName = AnonymousUserName;
                Password = AnonymousPassword;
            }
            else
            {
                UserName = _preferences.GetString("user", DefaultUserName);
                Password = _preferences.GetString("password", DefaultPassword);
            }

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
            editor.PutBoolean("anonymousAccess", AnonymousAccess);

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