using System;
using MonoTouch.Foundation;
using BitMobile.Application;
using BitMobile.Common.Application;
using BitMobile.Common;

namespace BitMobile.IOS
{
	public class Settings : ApplicationSettings
	{
		public override IApplicationSettings ReadSettings ()
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

			String p1 = NSUserDefaults.StandardUserDefaults.StringForKey ("URL");
			if (p1 == null) {
				p1 = defaultUrl;
				NSUserDefaults.StandardUserDefaults.SetString (p1, "URL");
			}
			this.BaseUrl = p1;
			
			String p2 = NSUserDefaults.StandardUserDefaults.StringForKey ("Application");
			if (p2 == null) {
				p2 = defaultApp;
				NSUserDefaults.StandardUserDefaults.SetString (p2, "Application");
			}
			this.ApplicationString = p2;

			this.Language = NSLocale.PreferredLanguages [0];

			this.UserName = NSUserDefaults.StandardUserDefaults.StringForKey ("User");
			this.Password = NSUserDefaults.StandardUserDefaults.StringForKey ("Password");
			
			this.ClearCacheOnStart = NSUserDefaults.StandardUserDefaults.BoolForKey ("ClearCacheOnStart");

			this.FtpPort = NSUserDefaults.StandardUserDefaults.StringForKey ("FtpPort");
			if (FtpPort == null)
				FtpPort = "21";

			NSUserDefaults.StandardUserDefaults.SetBool (ForceClearCache, "ClearCacheOnStart");

			NSUserDefaults.StandardUserDefaults.SetString (CoreInformation.CoreVersion.ToString (), "CoreVersion");	
			return this;
		}

		public override void WriteSettings ()
		{
			NSUserDefaults.StandardUserDefaults.SetString (this.BaseUrl, "URL");
			NSUserDefaults.StandardUserDefaults.SetString (this.ApplicationString, "Application");
			NSUserDefaults.StandardUserDefaults.SetString (this.UserName, "User");
			NSUserDefaults.StandardUserDefaults.SetString (this.Password, "Password");		
			NSUserDefaults.StandardUserDefaults.SetString (this.FtpPort, "FtpPort");			
			NSUserDefaults.StandardUserDefaults.SetString (CoreInformation.CoreVersion.ToString (), "CoreVersion");	

			if (ConfigName != null)
				NSUserDefaults.StandardUserDefaults.SetString (this.ConfigName, "Configuration");		
			if (ConfigVersion != null)
				NSUserDefaults.StandardUserDefaults.SetString (this.ConfigVersion, "Version");	
		}
	}
}

