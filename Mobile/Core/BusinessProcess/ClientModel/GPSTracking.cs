using System;
using BitMobile.Common;

namespace BitMobile.ClientModel
{
    public class GPSTracking
    {
        readonly Tracker _tracker;

        public GPSTracking(Tracker tracker)
        {
            _tracker = tracker;
            IsBestAccuracy = true;
            MinInterval = 60;
            MinDistance = 0;
            DistanceFilter = 3;
            SendInterval = 30;
        }

        // ReSharper disable once UnusedMember.Global
        public bool Start()
        {
            return _tracker.StartTracking(IsBestAccuracy, MinDistance, TimeSpan.FromSeconds(MinInterval));
        }

        // ReSharper disable once UnusedMember.Global
        public bool Stop()
        {
            return _tracker.StopTracking();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public bool IsBestAccuracy { get; set; }

        /// <summary>
        /// Interval in seconds
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public int MinInterval { get; set; }

        /// <summary>
        /// Distance in meters
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public int MinDistance { get; set; }

        /// <summary>
        /// Distance in meters
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public int DistanceFilter
        {
            // ReSharper disable once UnusedMember.Global
            get { return _tracker.DistanceFilter; }
            set { _tracker.DistanceFilter = value; }
        }

        /// <summary>
        /// Interval in seconds
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public int SendInterval
        {
            // ReSharper disable once UnusedMember.Global
            get { return _tracker.SendInterval; }
            set { _tracker.SendInterval = value; }
        }
    }
}

