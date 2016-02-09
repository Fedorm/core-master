using System;
using System.Text.RegularExpressions;
using BitMobile.Common.Application;
using BitMobile.Common.Develop;

namespace BitMobile.Application
{
    public abstract partial class ApplicationSettings : IApplicationSettings
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

        public ApplicationSettings()
        {
            CurrentSolutionType = DefaultSolutionType;
            HttpsDisabled = false;
        }

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

        public string Url
        {
            get
            {
                return BaseUrl + @"\device\";
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
                _baseUrl = new UriBuilder(value).Uri.AbsoluteUri;
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

        public bool BitMobileFormatterDisabled { get; private set; }

        public string DevelopersEmail { get; private set; }

        public SolutionType CurrentSolutionType { get; protected set; }

        public bool WebDavDisabled { get; private set; }

        public bool PingDisabled { get; private set; }

        public bool PushDisabled { get; private set; }
        public bool HttpsDisabled { get; set; }

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

        public string SolutionName
        {
            get
            {
                var uri = new UriBuilder(BaseUrl).Uri;
                string solutionName = uri.Segments[uri.Segments.Length - 1];

                solutionName = solutionName.Replace("/", "");
                solutionName = solutionName.Replace("\\", "");

                return solutionName;
            }
        }

        public TimeSpan? LogLifetime { get; private set; }

        public int? LogMinCount { get; private set; }

        public abstract IApplicationSettings ReadSettings();
        public abstract void WriteSettings();

        string ParseArguments(string val)
        {
            string[] args = val.Split('-');

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
                    case "oldformatter":
                        BitMobileFormatterDisabled = true;
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
                    case "disableping":
                        PingDisabled = true;
                        break;
                    case "disablepush":
                        PushDisabled = true;
                        break;
                    default:
                        if (Regex.IsMatch(arg, @"^re:\s*.+@.+$"))
                            DevelopersEmail = arg.Substring("re:".Length).Trim();
                        else if (Regex.IsMatch(arg, @"^loglifetime:\d+$"))
                        {
                            string daysString = arg.Substring("loglifetime:".Length);
                            int days;
                            if (int.TryParse(daysString, out days))
                                LogLifetime = new TimeSpan(days, 0, 0, 0);
                        }
                        else if (Regex.IsMatch(arg, @"^logmincount:\d+$"))
                        {
                            string countString = arg.Substring("logmincount:".Length);
                            int count;
                            if (int.TryParse(countString, out count))
                                LogMinCount = count;
                        }
                        break;
                }
            }

            ApplyArguments();

            return args[0].Trim().ToLower();
        }

        void ApplyArguments()
        {
            if (TimeStamp.Enabled != DevelopModeEnabled)
            {
                TimeStamp.Enabled = DevelopModeEnabled;
                TimeCollector.Enabled = DevelopModeEnabled;
                if (DevelopModeEnabled)
                {
                    TimeStamp.Write += Console.WriteLine;
                    TimeCollector.Write += Console.WriteLine;
                }
                else
                {
                    TimeStamp.Write -= Console.WriteLine;
                    TimeCollector.Write -= Console.WriteLine;
                }
            }
        }
    }

}

