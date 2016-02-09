using BitMobile.Application;
using BitMobile.Common;
using BitMobile.Common.Application;
using MonoTouch.Foundation;

namespace BitMobile.IOS
{
    public class Settings : ApplicationSettings
    {
        public const string KeyClearCacheOnStart = "ClearCacheOnStart";

        private const string KeyURL = "URL";

        private const string KeyApplication = "Application";

        private const string KeyUser = "User";

        private const string KeyPassword = "Password";

        private const string KeyFtpPort = "FtpPort";

        private const string KeyCoreVersion = "CoreVersion";

        public override IApplicationSettings ReadSettings()
        {
            NSUserDefaults.StandardUserDefaults.Init();

            BaseUrl = GetOrDefault(KeyURL, DefaultUrl);
            ApplicationString = GetOrDefault(KeyApplication, DefaultApplication);
            UserName = GetOrDefault(KeyUser, DefaultUserName);
            Password = GetOrDefault(KeyPassword, DefaultPassword);
            FtpPort = GetOrDefault(KeyFtpPort, DefaultFtpPort);            

            Language = BitMobile.Application.Translator.Translator.CheckLanguage(NSLocale.PreferredLanguages[0]);

            ClearCacheOnStart = NSUserDefaults.StandardUserDefaults.BoolForKey(KeyClearCacheOnStart);

            NSUserDefaults.StandardUserDefaults.SetBool(ForceClearCache, KeyClearCacheOnStart);

            NSUserDefaults.StandardUserDefaults.SetString(CoreInformation.CoreVersion.ToString(), KeyCoreVersion);
            return this;
        }

        public override void WriteSettings ()
        {
			NSUserDefaults.StandardUserDefaults.SetString (BaseUrl, KeyURL);
			NSUserDefaults.StandardUserDefaults.SetString (ApplicationString, KeyApplication);
			NSUserDefaults.StandardUserDefaults.SetString (UserName, KeyUser);
			NSUserDefaults.StandardUserDefaults.SetString (Password, KeyPassword);		
			NSUserDefaults.StandardUserDefaults.SetString (FtpPort, KeyFtpPort);			
            NSUserDefaults.StandardUserDefaults.SetString(CoreInformation.CoreVersion.ToString(), KeyCoreVersion);

            if (ConfigName != null)
				NSUserDefaults.StandardUserDefaults.SetString (ConfigName, "Configuration");		
            if (ConfigVersion != null)
				NSUserDefaults.StandardUserDefaults.SetString (ConfigVersion, "Version");	
		}
        
        private static string GetOrDefault(string key, string @default)
        {
            string value = NSUserDefaults.StandardUserDefaults.StringForKey(key);
            if (value == null)                            
                NSUserDefaults.StandardUserDefaults.SetString(value = @default, key);            
            return value;
        }
    }
}
