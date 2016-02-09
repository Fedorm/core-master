using System;
using BitMobile.Common;
using BitMobile.Common.Application.Tracking;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    public class GpsTracking
    {
        readonly ITracker _tracker;

        public GpsTracking(ITracker tracker)
        {
            _tracker = tracker;
            IsBestAccuracy = true;
            MinInterval = 60;
            MinDistance = 0;
            DistanceFilter = 3;
            SendInterval = 30;
        }

        public bool Start()
        {
            return _tracker.StartTracking(IsBestAccuracy, MinDistance, TimeSpan.FromSeconds(MinInterval));
        }

        public bool Stop()
        {
            return _tracker.StopTracking();
        }

        public bool IsBestAccuracy { get; set; }

        /// <summary>
        /// Interval in seconds
        /// </summary>
        public int MinInterval { get; set; }

        /// <summary>
        /// Distance in meters
        /// </summary>
        public int MinDistance { get; set; }

        /// <summary>
        /// Distance in meters
        /// </summary>
        public int DistanceFilter
        {
            get { return _tracker.DistanceFilter; }
            set { _tracker.DistanceFilter = value; }
        }

        /// <summary>
        /// Interval in seconds
        /// </summary>
        public int SendInterval
        {
            get { return _tracker.SendInterval; }
            set { _tracker.SendInterval = value; }
        }
    }
}

