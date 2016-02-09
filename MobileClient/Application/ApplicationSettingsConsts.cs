using BitMobile.Common.Application;

namespace BitMobile.Application
{
    public abstract partial class ApplicationSettings
    {
		// Client url. Example: "http://bitmobile2.cloudapp.net/demorutest"
		//public const string DefaultUrl = @"http://bitmobile1.bt/bitmobileX/platform"; // Required
		//public const string DefaultUrl = @"http://bitmobile1.bt/bitmobilex/sa"; // Required
		public const string DefaultUrl = @"http://192.168.105.188/bitmobile/devtest"; // Required
		//public const string DefaultUrl = @"http://bitmobile2.cloudapp.net/superagent"; // Required
						
		// Interface of solution initial screens. Enum: BitMobile, SuperAgent, SuperService, LandSuperService
        public const SolutionType DefaultSolutionType = SolutionType.BitMobile; // Required
		
		// Enable "Demo" button. If demo_enabled is true, demo_username and demo_password have to be set
        public const bool DemoEnabled = true; // Boolean: true or false. Default: false;
        public const string DemoUserName = "demo"; // Default: "demo"
        public const string DemoPassword = "demo"; // Default: "demo"
		
		// Default user name and password. Useful to developers. For demo applications use demo settings.
		public const string DefaultUserName = "sr"; // Default: ""
        public const string DefaultPassword = "sr"; // Default: ""
		
		// Application name with keys. Examples: "app -nowebdav -re:kvponomareva@1cbit.ru"
        public const string DefaultApplication = "app -d";//"-disableping -disablepush"; // Default: "app"
        //public const string DefaultApplication = "app -d -dw -s -re:dmitry0983@gmail.com -disableping";//"-disableping -disablepush"; // Default: "app"

        // Enable creating infobase from settings. Useful to developers
        public const bool EnableDefaultInfobases = true; // Boolean: true or false. Default: true
		
		// Enable set special name for default infobase.
        public const bool UseSpecialInfobaseName = false; // Boolean: true or false. Default: false
        public const string SpecialInfobaseName = ""; // Required if use_special_infobase_name is set
		
		// Ftp port. Obsolete. Useful for old clients like Land
        public const string DefaultFtpPort = "21"; // Default: "21"
    }
}