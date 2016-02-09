using System;

namespace BitMobile.Common.Device.Providers
{
    public interface ILocationProvider
    {
        IGpsCoordinate CurrentLocation { get; }         
        bool StartTracking(TimeSpan delay);
        bool StartTracking();
        bool StopTracking();
        bool UpdateLocation(int timeout);       
    }
}
