using System;
using GPSService.Tracking.Builder;

namespace GPSService.Tracking
{
    public class Segment
    {
        private readonly TrackingOptions _options;

        private double? _speed;
        private double? _distance;
        private bool? _isParking;
        
        /// <summary>
        /// Create a segment based on two dirty points
        /// </summary>
        public Segment(Point begin, Point end, TrackingOptions options)
        {
            _options = options;
            BeginTime = begin.Time;
            BeginLatitude = begin.Latitude;
            BeginLongitude = begin.Longitude;
            EndTime = end.Time;
            EndLatitude = end.Latitude;
            EndLongitude = end.Longitude;
            SatellitesCount = Math.Min(begin.SatellitesCount, end.SatellitesCount);

            IsBreak = Distance > _options.BreakDistance 
                && Duration.TotalSeconds >= _options.BreakDuration;
        }

        /// <summary>
        /// Create segment based on manual values
        /// </summary>
        /// <summary>Because this segment created manually, it cannot be a break</summary>
        public Segment(DateTime beginTime, double beginLatitude, double beginLongitude
            , DateTime endTime, double endLatitude, double endLongitude
            , int satellitesCount, TrackingOptions options)
        {
            _options = options;
            BeginTime = beginTime;
            BeginLatitude = beginLatitude;
            BeginLongitude = beginLongitude;
            EndTime = endTime;
            EndLatitude = endLatitude;
            EndLongitude = endLongitude;
            SatellitesCount = satellitesCount;

            IsBreak = false;
        }

        public DateTime BeginTime { get; private set; }

        public DateTime EndTime { get; private set; }

        public double BeginLatitude { get; private set; }

        public double BeginLongitude { get; private set; }

        public double EndLatitude { get; private set; }

        public double EndLongitude { get; private set; }

        public int SatellitesCount { get; private set; }

        public bool IsBreak { get; private set; }

        /// <summary>
        /// Speed in m/s
        /// </summary>
        public double Speed
        {
            get
            {
                if (_speed == null)
                {
                    var duration = (EndTime - BeginTime).TotalSeconds;
                    _speed = Distance / duration;
                }
                return _speed.Value;
            }
        }

        /// <summary>
        /// Distance in meters
        /// </summary>
        public double Distance
        {
            get
            {
                if (_distance == null)
                    _distance = EllipsoidWGS84.CalcDistance(this);
                return _distance.Value;
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public TimeSpan Duration
        {
            get { return EndTime - BeginTime; }
        }

        // ReSharper disable once UnusedMember.Global
        public bool IsParking
        {
            get
            {
                if (_isParking == null)
                {
                    if (Speed < _options.ParkingSpeed)
                        _isParking = Duration >= TimeSpan.FromMinutes(_options.ParkingInterval);
                    else
                        _isParking = false;
                }
                return _isParking.Value;
            }
        }
    }
}
