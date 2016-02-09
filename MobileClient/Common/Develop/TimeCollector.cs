using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BitMobile.Common.Develop
{
    public static class TimeCollector
    {
        private static readonly Stopwatch Current = new Stopwatch();

        static readonly Dictionary<string, Collector> TimeStamps = new Dictionary<string, Collector>();

        public static bool Enabled { get; set; }

        public static event Action<string> Write;

        public static void Start(string key)
        {
            Current.Start();
            if (Enabled)
                if (TimeStamps.ContainsKey(key))
                {
                    Collector collector = TimeStamps[key];
                    collector.Stopwatch.Start();
                    collector.Count += 1;
                }
                else
                    TimeStamps.Add(key, new Collector());
            Current.Stop();
        }

        public static void Pause(string key)
        {
            Current.Start();
            if (Enabled)
            {
                Collector c;
                if (TimeStamps.TryGetValue(key, out c))
                    c.Stopwatch.Stop();
            }
            Current.Stop();
        }

        public static void WriteAll()
        {
            if (Enabled)
            {
                if (Write != null)
                {
                    Write(string.Format("TIME_COLLECTOR: {0}", Current.Elapsed));

                    foreach (var stamp in TimeStamps)
                        Write(string.Format("TIME_COLLECTOR: {0} {1} {2}", stamp.Key, stamp.Value.Stopwatch.Elapsed,
                            stamp.Value.Count));
                }

                TimeStamps.Clear();
                Current.Restart();
            }
        }

        class Collector
        {
            public Collector()
            {
                Stopwatch = new Stopwatch();
                Stopwatch.Start();
                Count = 1;
            }

            public Stopwatch Stopwatch { get; private set; }
            public int Count { get; set; }
        }
    }
}
