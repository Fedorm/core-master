using System;

namespace BitMobile.Common.Application
{
    public interface IApplicationSettings
    {
        bool ClearCacheOnStart { get; }        
        string Url { get; }
        string BaseUrl { get; }
        string FtpPort { get; set; }
        string ApplicationString { get; }
        string Application { get; }
        string Language { get; }
        string UserName { get; set; }
        string Password { get; set; }
        string ConfigName { get; set; }
        string ConfigVersion { get; set; }
        bool DevelopModeEnabled { get; }
        bool TestAgentEnabled { get; }
        bool ForceClearCache { get; }
        bool SyncOnStart { get; }
        bool WaitDebuggerEnabled { get; }
        bool BackgoundLoadDisabled { get; }
        bool BitMobileFormatterDisabled { get; }
        bool PingDisabled { get;  }
        bool PushDisabled { get; }
        bool HttpsDisabled { get; set; }
        string DevelopersEmail { get; }
        SolutionType CurrentSolutionType { get; }
        bool WebDavDisabled { get; }
        bool IsInvalid { get; }
        string SolutionName { get; }
        TimeSpan? LogLifetime { get; }
        int? LogMinCount { get; }
        IApplicationSettings ReadSettings();
        void WriteSettings();
    }
}
