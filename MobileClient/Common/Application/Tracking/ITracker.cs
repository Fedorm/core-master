using System;

namespace BitMobile.Common.Application.Tracking
{
    public interface ITracker
    {
        int DistanceFilter { get; set; }
        int SendInterval { get; set; }
        bool StartTracking(bool bestAccuracy, int distance, TimeSpan interval);
        bool StopTracking();
    }
}
