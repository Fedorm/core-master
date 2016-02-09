using System;

namespace BitMobile.Common.Device.Providers
{
    public interface IGpsCoordinate
    {
        double Latitude { get; }
        double Longitude { get; }
        DateTime Time { get; }
        bool NotEmpty { get; }
        bool Empty { get; }
    }
}
