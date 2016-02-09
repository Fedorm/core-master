using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Utilities.Develop
{
    public static class TimeStamp
    {
        static Dictionary<string, DateTime> _timeStamps = new Dictionary<string, DateTime>();
        static StringBuilder _log = new StringBuilder();

        public static bool Enabled { get; set; }

        public static void Start(string key)
        {
            if (Enabled)
                if (_timeStamps.ContainsKey(key))
                    _timeStamps[key] = DateTime.Now;
                else
                    _timeStamps.Add(key, DateTime.Now);
        }

        public static void Write(string key, string description = "")
        {
            DateTime dt;
            if (Enabled)
                if (_timeStamps.TryGetValue(key, out dt))
                {
                    string report = PrepateReport(key, description, dt);
                    Console.WriteLine(report);
                }
        }

        public static void Log(string key, string description = "")
        {
            DateTime dt;
            if (Enabled)
                if (_timeStamps.TryGetValue(key, out dt))
                {
                    string report = PrepateReport(key, description, dt);
                    _log.AppendLine(report);
                }
        }

        public static void WriteLog()
        {
            if (Enabled)
            {
                Console.WriteLine(_log.ToString());
                _log.Clear();
            }
        }

        static string PrepateReport(string key, string description, DateTime dt)
        {
            TimeSpan tc = DateTime.Now - dt;
            string report = string.Format("TIME_STAMP: {0} {1} {2} ", key, tc, description);
            return report;
        }
    }
}
