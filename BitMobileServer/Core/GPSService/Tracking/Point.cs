using System;

namespace GPSService.Tracking
{
    public class Point
    {
        public Point(DateTime time, double latitude, double longitude, int satellitesCount)
        {
            SatellitesCount = satellitesCount;
            Longitude = longitude;
            Latitude = latitude;
            Time = time;
        }

        public DateTime Time { get; private set; }

        public double Latitude { get; private set; }

        public double Longitude { get; private set; }

        public int SatellitesCount { get; private set; }
    }
}
