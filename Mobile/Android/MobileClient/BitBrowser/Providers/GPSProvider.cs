using System;
using Android.Content;
using Android.Locations;
using Android.OS;
using BitMobile.Common;
using BitMobile.Droid.Backgrounding;
using BitMobile.Utilities;

namespace BitMobile.Droid.Providers
{
    class GpsProvider : Java.Lang.Object, ILocationProvider
    {
        private readonly BaseScreen _baseScreen;
        private DateTime? _startTime;
        Location _currentLocation;

        public GpsProvider(BaseScreen baseScreen)
        {
            _baseScreen = baseScreen;
        }

        #region ILocationProvider

        public GPSCoordinate CurrentLocation
        {
            get
            {
                return Convert(_currentLocation);
            }
        }

        public bool StartTracking()
        {
            _currentLocation = null;
            _startTime = DateTime.UtcNow;
            _baseScreen.BackgroundService.LocationChanged += BackgroundService_LocationChanged;
            return _baseScreen.BackgroundService.StartLocationUpdates(true, 0, TimeSpan.Zero);
        }

        public bool StopTracking()
        {
            _startTime = null;
            _baseScreen.BackgroundService.LocationChanged -= BackgroundService_LocationChanged;
            return _baseScreen.BackgroundService.StopLocationUpdates();
        }

        public bool UpdateLocation(int timeout)
        {
            return _currentLocation != null;
        }

        #endregion

        private void BackgroundService_LocationChanged(object sender, BaseService.CoordinatesChangedEventArgs e)
        {
            if (_startTime != null)
            {
                DateTime current = e.Location.Time.ToDateTime();
                if (current >= _startTime)
                    _currentLocation = e.Location;
            }
        }

        static GPSCoordinate Convert(Location location)
        {
            GPSCoordinate result = location != null
                ? new GPSCoordinate(location.Latitude, location.Longitude, location.Time.ToDateTime().ToLocalTime())
                : new GPSCoordinate();

            return result;
        }
    }
}