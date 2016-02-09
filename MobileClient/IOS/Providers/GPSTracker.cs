using System;
using System.Threading;
using BitMobile.Application.Tracking;
using MonoTouch.CoreLocation;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
    public class GPSTracker : Tracker
    {
        private readonly CLLocationManager _manager;
        private TimeSpan _interval;
        private Timer _timer;
        private bool _trackingStarted;

        public GPSTracker()
        {
            if (CLLocationManager.LocationServicesEnabled)
            {
                _manager = new CLLocationManager();
            }
        }

        public void RestoreMonitoring()
        {
            _manager.StartMonitoringSignificantLocationChanges();
        }

        public override bool StartTracking(bool bestAccuracy, int distance, TimeSpan interval)
        {
            if (_manager != null && !_trackingStarted)
            {
                _manager.DesiredAccuracy = bestAccuracy ? CLLocation.AccuracyBest : CLLocation.AccuracyKilometer;
                _manager.DistanceFilter = distance;
                _interval = interval;

                _manager.LocationsUpdated += HandleLocationsUpdated;

                if (new Version(UIDevice.CurrentDevice.SystemVersion).Major > 7)
                    _manager.RequestAlwaysAuthorization();

                _manager.StartUpdatingLocation();
                _trackingStarted = true;
                return true;
            }
            return false;
        }

        public override bool StopTracking()
        {
            if (_manager != null && _trackingStarted)
            {
                _manager.LocationsUpdated -= HandleLocationsUpdated;

                _manager.StopUpdatingLocation();
                _trackingStarted = false;
                return true;
            }
            return false;
        }

        private void HandleLocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
        {
            CLLocation location = e.Locations[e.Locations.Length - 1];
            DateTime time = DateTime.SpecifyKind(location.Timestamp, DateTimeKind.Unspecified);
            var args = new LocationEventArgs(location.Coordinate.Latitude, location.Coordinate.Longitude, time,
                location.Speed, location.Course, 0, location.Altitude);
            OnLocationChanged(args);

            if (_interval != TimeSpan.Zero && _timer == null)
            {
                _manager.StopUpdatingLocation();
                _timer = new Timer(TurnOnLocationManager, null, (int) _interval.TotalMilliseconds, 0);
            }
        }

        private void TurnOnLocationManager(object state)
        {
            _timer.Dispose();
            _timer = null;

            if (_trackingStarted)
                _manager.StartUpdatingLocation();
        }
    }
}