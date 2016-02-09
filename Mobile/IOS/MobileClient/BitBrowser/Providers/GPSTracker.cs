using System;
using MonoTouch.CoreLocation;
using BitMobile.Common;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Threading;

namespace BitMobile.IOS
{
	public class GPSTracker: Tracker
	{
		CLLocationManager _manager;
		bool _trackingStarted = false;
		TimeSpan _interval;
		Timer _timer;

		public GPSTracker ()
		{
			if (CLLocationManager.LocationServicesEnabled) {
				_manager = new CLLocationManager ();
			}
		}

		public void RestoreMonitoring ()
		{
			_manager.StartMonitoringSignificantLocationChanges ();
		}

		public override bool StartTracking (bool bestAccuracy, int distance, TimeSpan interval)
		{
			if (_manager != null && !_trackingStarted) {
				_manager.DesiredAccuracy = bestAccuracy ? CLLocation.AccuracyBest : CLLocation.AccuracyKilometer;
				_manager.DistanceFilter = distance;
				_interval = interval;

				_manager.LocationsUpdated += HandleLocationsUpdated;

				if (new Version (UIDevice.CurrentDevice.SystemVersion).Major > 7) 
					_manager.RequestAlwaysAuthorization ();

				_manager.StartUpdatingLocation ();
				_trackingStarted = true;
				return true;
			}
			return false;
		}

		public override bool StopTracking ()
		{
			if (_manager != null && _trackingStarted) {
				_manager.LocationsUpdated -= HandleLocationsUpdated;

				_manager.StopUpdatingLocation ();
				_trackingStarted = false;
				return true;
			}
			return false;
		}

		void HandleLocationsUpdated (object sender, CLLocationsUpdatedEventArgs e)
		{
			var location = e.Locations [e.Locations.Length - 1];
			DateTime time = DateTime.SpecifyKind (location.Timestamp, DateTimeKind.Unspecified);
			var args = new LocationEventArgs (location.Coordinate.Latitude, location.Coordinate.Longitude, time, location.Speed, location.Course, 0, location.Altitude);
			OnLocationChanged (args);

			if (_interval != TimeSpan.Zero && _timer == null) {
				_manager.StopUpdatingLocation ();
				_timer = new Timer (new TimerCallback(TurnOnLocationManager), null, (int)_interval.TotalMilliseconds, 0);
			}
		}

		void TurnOnLocationManager (object state)
		{
			_timer.Dispose ();
			_timer = null;

			if (_trackingStarted)
				_manager.StartUpdatingLocation ();
		}
	}
}

