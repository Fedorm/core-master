using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Common
{
    public interface ILocationProvider
    {
        GPSCoordinate CurrentLocation { get; }         
        bool StartTracking();
        bool StopTracking();
        bool UpdateLocation(int timeout);       
    }
}
