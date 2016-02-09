using System;
using BitMobile.Application.Tracking;
using MonoTouch.UIKit;
using MonoTouch.CoreLocation;
using BitMobile.Common.Device.Providers;

namespace BitMobile.IOS
{
    public class GpsProvider : ILocationProvider
    {
        private readonly CLLocationManager _manager;
        private bool _trackingStarted;
        private GpsCoordinate _currentLocation;
        private DateTime _startTime;

        public GpsProvider()
        {
            if (CLLocationManager.LocationServicesEnabled)
            {
                _manager = new CLLocationManager
                {
                    DesiredAccuracy = CLLocation.AccurracyBestForNavigation,
                    Delegate = new LocationManagerDelegate()
                };
            }
        }

        #region ILocationProvider implementation

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
            if (_manager != null && _trackingStarted)
            {
                _manager.StopUpdatingLocation();
                _trackingStarted = false;
                return true;
            }
            return false;
        }

        public bool UpdateLocation(int timeout)
        {
            bool result = false;

            if (_manager != null)
            {
                DateTime lastTime = DateTime.Now;
                while (lastTime.AddSeconds(timeout) > DateTime.Now)
                {
                    CLLocation location = _manager.Location;

                    if (location != null)
                    {
                        DateTime time = DateTime.SpecifyKind(location.Timestamp, DateTimeKind.Unspecified);
                        if (DateTime.UtcNow < time.AddMinutes(5))
                        {
                            _currentLocation = new GpsCoordinate(location.Coordinate.Latitude, location.Coordinate.Longitude, time);
                            result = true;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        public IGpsCoordinate CurrentLocation
        {
            get
            {
                if (_manager != null)
                {
                    CLLocation location = _manager.Location;
                    if (location != null)
                    {
                        DateTime time = DateTime.SpecifyKind(location.Timestamp, DateTimeKind.Unspecified);
                        if (time >= _startTime)
                            _currentLocation = new GpsCoordinate(location.Coordinate.Latitude, location.Coordinate.Longitude, time);
                    }
                }

                return _currentLocation;
            }
        }

        #endregion

        private bool Start()
        {
            if (_manager != null && !_trackingStarted)
            {
                _currentLocation = new GpsCoordinate();

                if (new Version(UIDevice.CurrentDevice.SystemVersion).Major > 7)
                    _manager.RequestAlwaysAuthorization();

                _manager.StartUpdatingLocation();
                _trackingStarted = true;
                return true;
            }
            return false;
        }
    }

    internal class LocationManagerDelegate : CLLocationManagerDelegate
    {
        public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
        {
        }
    }
}

