using System;

using BitMobile.Common;
using BitMobile.Application;
using BitMobile.Utilities.Exceptions;
using BitMobile.Utilities.Translator;

namespace BitMobile.ClientModel
{
    public class GPS
    {
        const int DEFAULT_TIMEOUT = 3;

        ILocationProvider _provider;
        GPSCoordinate _current;
        System.DateTime _lastRequest;

        public GPS(ILocationProvider provider)
        {
            _provider = provider;
        }

        [Obsolete]
        public Nullable<double> Latitude
        {
            get
            {
                return CurrentLocation.Latitude;
            }
        }

        [Obsolete]
        public Nullable<double> Longitude
        {
            get
            {
                return CurrentLocation.Longitude;
            }
        }

        public GPSCoordinate CurrentLocation
        {
            get
            {
                RefreshCurrentLocation();
                return _current;
            }
        }

        public bool Update()
        {
            return Update(DEFAULT_TIMEOUT);
        }

        public bool Update(int timeout)
        {
            return _provider.UpdateLocation(timeout);
        }

        public bool StartTracking()
        {
            return _provider.StartTracking();
        }

        public bool StopTracking()
        {
            return _provider.StopTracking();
        }

        void RefreshCurrentLocation()
        {
            if (System.DateTime.Now > _lastRequest.AddSeconds(DEFAULT_TIMEOUT))
                _current = _provider.CurrentLocation;
        }
    }

}

