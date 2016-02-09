using System;
using BitMobile.Application.Extensions;
using BitMobile.Application.Tracking;
using BitMobile.Droid.Backgrounding;

namespace BitMobile.Droid.Providers
{
    class GpsTracker : Tracker
    {
        readonly BaseScreen _baseScreen;

        public GpsTracker(BaseScreen context)
        {
            _baseScreen = context;
        }

        public override bool StartTracking(bool bestAccuracy, int distance, TimeSpan interval)
        {
            if (_baseScreen.BackgroundService == null)
                return false;

            _baseScreen.BackgroundService.LocationChanged += BackgroundService_LocationChanged;

            return _baseScreen.BackgroundService.StartLocationUpdates(bestAccuracy, distance, interval);
        }

        public override bool StopTracking()
        {
            if (_baseScreen.BackgroundService == null)
                return false;

            _baseScreen.BackgroundService.LocationChanged -= BackgroundService_LocationChanged;
            return _baseScreen.BackgroundService.StopLocationUpdates();
        }

        void BackgroundService_LocationChanged(object sender, BaseService.CoordinatesChangedEventArgs e)
        {
            var args = new LocationEventArgs(e.Location.Latitude
                , e.Location.Longitude
                , e.Location.Time.ToDateTime()
                , e.Location.Speed
                , e.Location.Bearing
                , e.SatellitesCount
                , e.Location.Altitude);

            OnLocationChanged(args);
        }
    }
}
