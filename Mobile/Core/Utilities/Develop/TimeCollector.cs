using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Utilities.Develop
{
    public static class TimeCollector
    {
        static Dictionary<string, Collector> _timeStamps = new Dictionary<string, Collector>();

        public static bool Enabled { get; set; }

        public static void Start(string key)
        {
            if (Enabled)
                if (_timeStamps.ContainsKey(key))
                {
                    if (_timeStamps[key].Stopped)
                        _timeStamps[key] = new Collector();
                    else
                        _timeStamps[key].StartTime = DateTime.Now;
                }
                else
                    _timeStamps.Add(key, new Collector());
        }

        public static void Pause(string key)
        {
            if (Enabled)
                LogTime(key);
        }

        public static void Stop(string key)
        {
            if (Enabled)
            {
                Collector c = LogTime(key);
                if (c != null)
                    c.Stopped = true;
            }
        }

        public static void Write(string key, string description = "")
        {
            Collector dt;
            if (Enabled)
                if (_timeStamps.TryGetValue(key, out dt))
                {
                    string report = string.Format("TIME_COLLECTOR: {0} {1} {2} ", key, dt.TotalTime, description);
                    Console.WriteLine(report);
                }
        }

        static Collector LogTime(string key)
        {
            Collector c;
            if (_timeStamps.TryGetValue(key, out c))
            {
                if (c.StartTime != DateTime.MinValue)
                    c.TotalTime += DateTime.Now - c.StartTime;

                c.StartTime = DateTime.MinValue;
            }
            return c;
        }

        class Collector
        {
            public Collector()
            {
                StartTime = DateTime.Now;
            }

            public DateTime StartTime { get; set; }
            public TimeSpan TotalTime { get; set; }
            public bool Stopped { get; set; }
        }
    }
}
