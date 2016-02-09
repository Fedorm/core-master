using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BitMobile.Common.Develop
{
    public static class TimeStamp
    {
        private static readonly Stopwatch Current = new Stopwatch();
        private static readonly Dictionary<string, Stopwatch> TimeStamps = new Dictionary<string, Stopwatch>();
        private static readonly StringBuilder LogString = new StringBuilder();

        public static bool Enabled { get; set; }

        public static event Action<string> Write;

        public static void Start(string key)
        {
            if (Enabled)
            {
                Current.Start();

                Stopwatch stopwatch;
                if (!TimeStamps.TryGetValue(key, out stopwatch))
                {
                    stopwatch = new Stopwatch();
                    TimeStamps.Add(key, stopwatch);
                }
                else
                    stopwatch.Reset();

                stopwatch.Start();

                Current.Stop();
            }
        }

        public static void Log(string key, string description = "")
        {
            if (Enabled)
            {
                Current.Start();
                Stopwatch stopwatch;
                if (TimeStamps.TryGetValue(key, out stopwatch))
                {
                    string report = PrepateReport(key, description, stopwatch);
                    LogString.AppendLine(report);
                }
                Current.Stop();
            }
        }

        public static void WriteAll()
        {
            if (Enabled)
            {
                if (Write != null)
                {
                    Write(LogString.ToString());
                    Write(string.Format("TIME_STAMP: {0} ", Current.Elapsed));
                    Current.Reset();
                }
                LogString.Clear();
            }
        }

        static string PrepateReport(string key, string description, Stopwatch stopwatch)
        {
            string report = string.Format("TIME_STAMP: {0} {1} {2} ", key, stopwatch.Elapsed, description);
            return report;
        }
    }
}
