using System;

namespace BitMobile.Common
{
    public struct GPSCoordinate
    {
        double _latitude;
        public double Latitude
        {
            get { return _latitude; }
        }

        double _longitude;
        public double Longitude
        {
            get { return _longitude; }
        }

        DateTime _time;
        public DateTime Time
        {
            get { return _time; }
        }

        bool _notEmpty;
        public bool NotEmpty
        {
            get { return _notEmpty; }
        }

        public GPSCoordinate(double latitude, double longitude, DateTime time)
        {
            _latitude = latitude;
            _longitude = longitude;
            _time = time;
            _notEmpty = true;
        }
    }
}

