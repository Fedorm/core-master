using System;
using System.Threading;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreLocation;
using BitMobile.Common;

namespace BitMobile.IOS
{
	public class GPSProvider : ILocationProvider
	{
		CLLocationManager _manager;
		bool _trackingStarted = false;
		GPSCoordinate _currentLocation;
		DateTime _startTime;

		public GPSProvider ()
		{
			if (CLLocationManager.LocationServicesEnabled) {
				_manager = new CLLocationManager ();
				_manager.DesiredAccuracy = CLLocation.AccurracyBestForNavigation;
				_manager.Delegate = new LocationManagerDelegate (this);
			}
		}

		#region ILocationProvider implementation

		public bool StartTracking ()
		{
			if (_manager != null && !_trackingStarted) {
				_startTime = DateTime.UtcNow;
				_currentLocation = new GPSCoordinate ();
				_manager.StartUpdatingLocation ();

				if (new Version (UIDevice.CurrentDevice.SystemVersion).Major > 7) 
					_manager.RequestAlwaysAuthorization ();

				_trackingStarted = true;
				return true;
			}
			return false;
		}

		public bool StopTracking ()
		{
			if (_manager != null && _trackingStarted) {
				_manager.StopUpdatingLocation ();
				_trackingStarted = false;
				return true;
			}
			return false;
		}

		public bool UpdateLocation (int timeout)
		{
			bool result = false;

			if (_manager != null) {
				DateTime lastTime = DateTime.Now;
				while (lastTime.AddSeconds (timeout) > DateTime.Now) {
					CLLocation location = _manager.Location;

					if (location != null) {
						DateTime time = DateTime.SpecifyKind (location.Timestamp, DateTimeKind.Unspecified);
						if (DateTime.UtcNow < time.AddMinutes (5)) {
							_currentLocation = new GPSCoordinate (location.Coordinate.Latitude, location.Coordinate.Longitude, time);
							result = true;
							break;
						}
					}
				}
			}
			return result;
		}

		public GPSCoordinate CurrentLocation {
			get {
				if (_manager != null) {
					CLLocation location = _manager.Location;
					if (location != null) {
						DateTime time = DateTime.SpecifyKind (location.Timestamp, DateTimeKind.Unspecified);
						if (time >= _startTime)
							_currentLocation = new GPSCoordinate (location.Coordinate.Latitude, location.Coordinate.Longitude, time);
					}					
				}

				return _currentLocation;
			}
		}

		#endregion
	}

	internal class LocationManagerDelegate : CLLocationManagerDelegate
	{
		GPSProvider _manager;

		public LocationManagerDelegate (GPSProvider manager)
		{
			_manager = manager;
		}

		public override void LocationsUpdated (CLLocationManager manager, CLLocation[] locations)
		{
		}
	}
}

