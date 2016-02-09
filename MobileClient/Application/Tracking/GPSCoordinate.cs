using System;
using BitMobile.Common.Device.Providers;

namespace BitMobile.Application.Tracking
{
    public struct GpsCoordinate : IGpsCoordinate
    {
        public double Latitude { get; private set; }

        public double Longitude { get; private set; }

        public DateTime Time { get; private set; }

        readonly bool _notEmpty;
        public bool NotEmpty
        {
            get { return _notEmpty; }
        }

        public bool Empty
        {
            get { return !_notEmpty; }
        }

        public GpsCoordinate(double latitude, double longitude, DateTime time)
            : this()
        {
            try
            {
                checked
                {
                    double deg = Math.Pow(10, 8);
                    double lat = Math.Floor(latitude * deg);
                    double len = Math.Floor(longitude * deg);

                    Latitude = lat / deg;
                    Longitude = len / deg;   
                }
            }
            catch (OverflowException)
            {
                Latitude = latitude;
                Longitude = longitude;
            }

            Time = time;
            _notEmpty = true;
        }
    }
}

