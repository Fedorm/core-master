using System;
using Android.Locations;
using BitMobile.Application.Extensions;
using BitMobile.Application.Tracking;
using BitMobile.Common.Device.Providers;
using BitMobile.Droid.Backgrounding;

namespace BitMobile.Droid.Providers
{
    class GpsProvider : Java.Lang.Object, ILocationProvider
    {
        private readonly BaseScreen _baseScreen;
        private DateTime _startTime = DateTime.MinValue;
        private bool _started;
        private Location _currentLocation;

        public GpsProvider(BaseScreen baseScreen)
        {
            _baseScreen = baseScreen;
        }

        #region ILocationProvider

        public IGpsCoordinate CurrentLocation
        {
            get
            {
                return Convert(_currentLocation);
            }
        }

        public bool StartTracking(TimeSpan delay)
        {
            _startTime = DateTime.UtcNow - delay;
            return Start();
        }

        public bool StartTracking()
        {
            _startTime = DateTime.MinValue;
            return Start();
        }

        public bool StopTracking()
        {
            if (_started)
            {
                if (_baseScreen.BackgroundService == null)
                    return false;

                _started = false;                
                _baseScreen.BackgroundService.LocationChanged -= BackgroundService_LocationChanged;
                return _baseScreen.BackgroundService.StopLocationUpdates();
            }
            return false;
        }

        public bool UpdateLocation(int timeout)
        {
            return _currentLocation != null;
        }

        #endregion

        private bool Start()
        {
            if (!_started)
            {
                if (_baseScreen.BackgroundService == null)
                    return false;

                _currentLocation = null;
                _started = true;
                _baseScreen.BackgroundService.LocationChanged += BackgroundService_LocationChanged;
                return _baseScreen.BackgroundService.StartLocationUpdates(true, 0, TimeSpan.Zero);
            }
            return false;
        }

        private void BackgroundService_LocationChanged(object sender, BaseService.CoordinatesChangedEventArgs e)
        {
            if (_started)
            {
                DateTime current = e.Location.Time.ToDateTime();
                if (current >= _startTime)
                    _currentLocation = e.Location;
            }
        }

        static GpsCoordinate Convert(Location location)
        {
            GpsCoordinate result = location != null
                ? new GpsCoordinate(location.Latitude, location.Longitude, location.Time.ToDateTime().ToLocalTime())
                : new GpsCoordinate();

            return result;
        }
    }
}