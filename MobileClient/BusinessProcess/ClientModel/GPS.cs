using System;
using BitMobile.Common.Device.Providers;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    public class Gps
    {
        private const int DefaultTimeout = 3;

        private readonly ILocationProvider _provider;
        private IGpsCoordinate _current;
        private DateTime _lastRequest = DateTime.MinValue;

        public Gps(ILocationProvider provider)
        {
            _provider = provider;
        }

        [Obsolete]
        public double? Latitude
        {
            get
            {
                return CurrentLocation.Latitude;
            }
        }

        [Obsolete]
        public double? Longitude
        {
            get
            {
                return CurrentLocation.Longitude;
            }
        }

        public IGpsCoordinate CurrentLocation
        {
            get
            {
                RefreshCurrentLocation();
                return _current;
            }
        }

        public bool Update()
        {
            return Update(DefaultTimeout);
        }

        public bool Update(int timeout)
        {
            return _provider.UpdateLocation(timeout);
        }

        public bool StartTracking()
        {
            return StartTracking(0);
        }

        public bool StartTracking(int delay)
        {
            if (delay >= 0)
                return _provider.StartTracking(TimeSpan.FromSeconds(delay));
            return _provider.StartTracking();
        }

        public bool StopTracking()
        {
            return _provider.StopTracking();
        }

        void RefreshCurrentLocation()
        {
            if (DateTime.Now > _lastRequest.AddSeconds(DefaultTimeout))
                _current = _provider.CurrentLocation;
        }
    }

}

