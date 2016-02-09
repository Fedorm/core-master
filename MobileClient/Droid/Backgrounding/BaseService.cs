using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Locations;

namespace BitMobile.Droid.Backgrounding
{
    [Service]
    public class BaseService : Service, ILocationListener, GpsStatus.IListener
    {
        IBinder _binder;
        readonly LocationManager _networkLocationManager = Android.App.Application.Context.GetSystemService(LocationService) as LocationManager;
        readonly LocationManager _gpsLocationManager = Android.App.Application.Context.GetSystemService(LocationService) as LocationManager;
        readonly Dictionary<string, Availability> _providerAvailabilities = new Dictionary<string, Availability>();
        bool _trackingStarted;
        private int _satellitesCount;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            _binder = new ServiceBinder(this);
            return _binder;
        }

        public event EventHandler<CoordinatesChangedEventArgs> LocationChanged;

        public bool StartLocationUpdates(bool bestAccuracy, int distance, TimeSpan interval)
        {
            if (!_trackingStarted)
            {
                Location lastKnownLocation = null;

                if (_networkLocationManager.AllProviders.Contains(LocationManager.NetworkProvider))
                {
                    _networkLocationManager.RequestLocationUpdates(LocationManager.NetworkProvider
                        , (int)interval.TotalMilliseconds, distance, this);
                    _networkLocationManager.AddGpsStatusListener(this);

                    lastKnownLocation = _networkLocationManager.GetLastKnownLocation(LocationManager.NetworkProvider);
                }

                if (bestAccuracy && _gpsLocationManager.AllProviders.Contains(LocationManager.GpsProvider))
                {
                    _gpsLocationManager.RequestLocationUpdates(LocationManager.GpsProvider
                        , (int)interval.TotalMilliseconds, distance, this);
                    _gpsLocationManager.AddGpsStatusListener(this);

                    Location gpsLastKnownLocation = _gpsLocationManager.GetLastKnownLocation(LocationManager.GpsProvider);
                    if (gpsLastKnownLocation != null)
                        if (lastKnownLocation == null || lastKnownLocation.Time < gpsLastKnownLocation.Time)
                            lastKnownLocation = gpsLastKnownLocation;
                }

                _trackingStarted = true;

                if (lastKnownLocation != null)
                    HandleLocationChanged(lastKnownLocation);
                return true;
            }
            return false;
        }

        public bool StopLocationUpdates()
        {
            if (_trackingStarted && LocationChanged == null)
            {
                _networkLocationManager.RemoveUpdates(this);
                _gpsLocationManager.RemoveUpdates(this);
                _trackingStarted = false;
                return true;
            }
            return false;
        }

        #region ILocationListener

        public void OnLocationChanged(Location location)
        {
            Availability availability;
            if (!_providerAvailabilities.TryGetValue(location.Provider, out availability))
                availability = Availability.Available;

            if (availability == Availability.Available)
                HandleLocationChanged(location);
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            _providerAvailabilities[provider] = status;
        }

        #endregion

        #region GpsStatus.IListener

        public void OnGpsStatusChanged(GpsEvent e)
        {
            int satellitesInView = 0;
            int satellitesInUse = 0;
            var gpsStatus = _gpsLocationManager.GetGpsStatus(null);
            var iterator = gpsStatus.Satellites.Iterator();
            while (iterator.HasNext)
            {
                satellitesInView++;
                var satelite = (GpsSatellite)iterator.Next();
                if (satelite.UsedInFix())
                    satellitesInUse++;// in present time we cannot find any satellites in use, so satellitesInUse is useless
            }
            _satellitesCount = satellitesInView;
        }
        #endregion

        private void HandleLocationChanged(Location location)
        {
            var args = new CoordinatesChangedEventArgs(location, GetSatellitesCount(location));
            if (LocationChanged != null)
                LocationChanged(this, args);
        }

        private int GetSatellitesCount(Location location)
        {
            int result = 0;
            if (location.Provider == LocationManager.GpsProvider)
                result = _satellitesCount;
            return result;
        }

        public class CoordinatesChangedEventArgs : LocationChangedEventArgs
        {
            public CoordinatesChangedEventArgs(Location location, int satellitesCount)
                : base(location)
            {
                SatellitesCount = satellitesCount;
            }

            public int SatellitesCount { get; private set; }
        }
    }
}