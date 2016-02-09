using System;
using BitMobile.Common;
using BitMobile.Droid.Backgrounding;
using BitMobile.Utilities;

namespace BitMobile.Droid.Providers
{
    class GPSTracker : Tracker
    {
        readonly BaseScreen _baseScreen;

        public GPSTracker(BaseScreen context)
        {
            _baseScreen = context;
        }

        public override bool StartTracking(bool bestAccuracy, int distance, TimeSpan interval)
        {
            _baseScreen.BackgroundService.LocationChanged += BackgroundService_LocationChanged;

            return _baseScreen.BackgroundService.StartLocationUpdates(bestAccuracy, distance, interval);
        }

        public override bool StopTracking()
        {
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
