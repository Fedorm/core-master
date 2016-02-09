using BitMobile.Utilities.Develop;
using System;
using System.Text.RegularExpressions;

namespace BitMobile.Application
{
    public abstract class ApplicationSettings
    {
        bool _clearCacheOnStart;
        string _baseUrl;
        string _applicationString;
        string _application;
        string _language;
        string _userName;
        string _password;
        string _configName;
        string _configVersion;

        public bool ClearCacheOnStart
        {
            get
            {
                return _clearCacheOnStart;
            }
            set
            {
                _clearCacheOnStart = value;
            }
        }

        public bool AnonymousAccess { get; protected set; }

        public string Url
        {
            get
            {
                return _baseUrl + @"\device\";
            }
        }

        public string BaseUrl
        {
            get
            {
                return _baseUrl;
            }
            protected set
            {
                _baseUrl = value;
            }
        }

        public string FtpPort { get; set; }

        public string ApplicationString
        {
            get
            {
                return _applicationString;
            }
            protected set
            {
                _applicationString = value;
                _application = ParseArguments(value);
            }
        }

        public string Application
        {
            get
            {
                return _application;
            }
        }

        public string Language
        {
            get
            {
                return _language;
            }
            protected set
            {
                _language = value.ToLower();
            }
        }

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                this._userName = value;
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                this._password = value;
            }
        }

        public string ConfigName
        {
            get
            {
                return _configName;
            }
            set
            {
                _configName = value;
            }
        }

        public string ConfigVersion
        {
            get
            {
                return _configVersion;
            }
            set
            {
                _configVersion = value;
            }
        }

        public bool DevelopModeEnabled { get; private set; }

        public bool TestAgentEnabled { get; private set; }

        public bool ForceClearCache { get; private set; }

        public bool SyncOnStart { get; private set; }

        public bool WaitDebuggerEnabled { get; private set; }

        public bool BackgoundLoadDisabled { get; private set; }

        public string DevelopersEmail { get; private set; }

        public SolutionType CurrentSolutionType { get; protected set; }

        public bool WebDavDisabled { get; private set; }

        public bool IsInvalid
        {
            get
            {
                return string.IsNullOrWhiteSpace(_userName)
                || string.IsNullOrWhiteSpace(_password)
                || string.IsNullOrWhiteSpace(_baseUrl)
                || string.IsNullOrWhiteSpace(_applicationString)
                || string.IsNullOrWhiteSpace(_language);
            }
        }

        public abstract ApplicationSettings ReadSettings();
        public abstract void WriteSettings();

        string ParseArguments(string val)
        {
            string[] args = val.Split('-');

            DevelopModeEnabled = false;
            TestAgentEnabled = false;
            ForceClearCache = false;
            SyncOnStart = false;
            BackgoundLoadDisabled = false;
            WaitDebuggerEnabled = false;
            WebDavDisabled = false;

            for (int i = 1; i < args.Length; i++)
            {
                string arg = args[i].Trim().ToLower();
                switch (arg)
                {
                    case "d":
                        DevelopModeEnabled = true;
                        break;
                    case "t":
                        TestAgentEnabled = true;
                        break;
                    case "c":
                        ForceClearCache = true;
                        break;
                    case "s":
                        SyncOnStart = true;
                        break;
                    case "dw":
                        WaitDebuggerEnabled = true;
                        DevelopModeEnabled = true;
                        break;
                    case "nowebdav":
                        WebDavDisabled = true;
                        break;
                    case "backgroundloaddisabled":
                        BackgoundLoadDisabled = true;
                        break;
                    case "bitmobile":
                        CurrentSolutionType = SolutionType.BitMobile;
                        break;
                    case "superagent":
                        CurrentSolutionType = SolutionType.SuperAgent;
                        break;
                    case "superservice":
                        CurrentSolutionType = SolutionType.SuperService;
                        break;
                    default:
                        if (Regex.IsMatch(arg, @"^re:\s*.+@.+$"))
                            this.DevelopersEmail = arg.Substring(3).Trim();
                        break;
                }
            }

            ApplyArguments();

            return args[0].Trim().ToLower();
        }

        void ApplyArguments()
        {
            TimeStamp.Enabled = DevelopModeEnabled;
            TimeCollector.Enabled = DevelopModeEnabled;
        }
    }

}

