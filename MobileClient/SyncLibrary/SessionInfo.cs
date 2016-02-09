using System;

namespace BitMobile.SyncLibrary
{
    [Serializable]
    public class SessionInfo
    {
        String _userId;
        public String UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        String _configName;
        public String ConfigName
        {
            get { return _configName; }
            set { _configName = value; }
        }

        String _configVersion;
        public String ConfigVersion
        {
            get { return _configVersion; }
            set { _configVersion = value; }
        }
    }
}