using System;
using MonoTouch.Foundation;
using BitMobile.Application;
using BitMobile.DataAccessLayer;
using BitMobile.Utilities.Translator;

namespace BitMobile.IOS
{
	public class Settings : ApplicationSettings
	{
		public const string KeyClearCacheOnStart = "ClearCacheOnStart";

		const string KeyURL = "URL";

		const string KeyApplication = "Application";

		const string KeyUser = "User";

		const string KeyPassword = "Password";

		const string KeyFtpPort = "FtpPort";

		const string KeyCoreVersion = "CoreVersion";

		public override ApplicationSettings ReadSettings ()
		{
			#if DEBUG

			String defaultUrl = "http://192.168.0.2/bitmobile/test";

			String defaultApp = "app -c -d -re:kugushew@gmail.com";

			#else

			//String defaultUrl = "http://bitmobile.cloudapp.net/demorup";
			//String defaultUrl = "http://bitmobile2.cloudapp.net/demorutest";
			//String defaultUrl = "http://192.168.0.140/bitmobile/fortest";

			//this.CurrentSolutionType = SolutionType.BitMobile;
			//this.CurrentSolutionType = SolutionType.SuperAgent;
			//this.CurrentSolutionType = SolutionType.SuperService;

			//1. Uncomment url and solution name
			//2. Change en.lproj/InfoPlist.strings and ru.lproj/InfoPlist.strings 
			//3. Change Incons in Info.plist

			String defaultApp = "app";

			#endif

			NSUserDefaults.StandardUserDefaults.Init ();

			String p1 = NSUserDefaults.StandardUserDefaults.StringForKey (KeyURL);
			if (p1 == null) {
				p1 = defaultUrl;
				NSUserDefaults.StandardUserDefaults.SetString (p1, KeyURL);
			}
			this.BaseUrl = p1;
			
			String p2 = NSUserDefaults.StandardUserDefaults.StringForKey (KeyApplication);
			if (p2 == null) {
				p2 = defaultApp;
				NSUserDefaults.StandardUserDefaults.SetString (p2, KeyApplication);
			}
			this.ApplicationString = p2;

			this.Language = NSLocale.PreferredLanguages [0];

			this.UserName = NSUserDefaults.StandardUserDefaults.StringForKey (KeyUser);
			this.Password = NSUserDefaults.StandardUserDefaults.StringForKey (KeyPassword);
			
			this.ClearCacheOnStart = NSUserDefaults.StandardUserDefaults.BoolForKey (KeyClearCacheOnStart);

			this.FtpPort = NSUserDefaults.StandardUserDefaults.StringForKey (KeyFtpPort);
			if (FtpPort == null)
				FtpPort = "21";

			NSUserDefaults.StandardUserDefaults.SetBool (ForceClearCache, KeyClearCacheOnStart);

			NSUserDefaults.StandardUserDefaults.SetString (CoreInformation.CoreVersion.ToString (), KeyCoreVersion);	
			return this;
		}

		public override void WriteSettings ()
		{
			NSUserDefaults.StandardUserDefaults.SetString (this.BaseUrl, KeyURL);
			NSUserDefaults.StandardUserDefaults.SetString (this.ApplicationString, KeyApplication);
			NSUserDefaults.StandardUserDefaults.SetString (this.UserName, KeyUser);
			NSUserDefaults.StandardUserDefaults.SetString (this.Password, KeyPassword);		
			NSUserDefaults.StandardUserDefaults.SetString (this.FtpPort, KeyFtpPort);		
	
			if (ConfigName != null)
				NSUserDefaults.StandardUserDefaults.SetString (this.ConfigName, "Configuration");		
			if (ConfigVersion != null)
				NSUserDefaults.StandardUserDefaults.SetString (this.ConfigVersion, "Version");	
			NSUserDefaults.StandardUserDefaults.SetString (CoreInformation.CoreVersion.ToString (), KeyCoreVersion);	
		}
	}
}

