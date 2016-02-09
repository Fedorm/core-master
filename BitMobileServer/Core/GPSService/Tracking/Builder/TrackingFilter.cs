using System;
using System.Collections.Generic;
using System.Linq;

namespace GPSService.Tracking.Builder
{
    class TrackingFilter
    {
        private readonly TrackingOptions _options;

        public TrackingFilter(TrackingOptions options)
        {
            _options = options;
        }

        public bool ValidateSegment(Segment current, List<Segment> list)
        {
            bool result = NotZero(current);

            if (result)
                result = NotDublicate(current);

            if (result)
                result = TimeIsCorrect(current);

            if (result)
                result = SatellitesEnough(current);

            if (result)
                result = CorrectSpeed(current);
            
            if (result)
                result = CorrectAcceleration(current, list);

            if (result)
                result = DistanceEnough(current);

            return result;
        }

        private bool NotZero(Segment current)
        {
            return Math.Abs(current.EndLatitude) > 0 
                && Math.Abs(current.EndLongitude) > 0;
        }

        private bool TimeIsCorrect(Segment current)
        {
            return current.BeginTime < current.EndTime;
        }

        private bool NotDublicate(Segment current)
        {
            return current.Distance > 1 && current.BeginTime != current.EndTime;
        }

        private bool SatellitesEnough(Segment current)
        {
            return current.SatellitesCount == 0 // network provider
                || current.SatellitesCount >= _options.MinSatellites;
        }

        private bool DistanceEnough(Segment current)
        {
            return current.Distance > _options.MinDistance;
        }

        private bool CorrectSpeed(Segment current)
        {
            return current.Speed <= _options.MaxSpeed;
        }

        private bool CorrectAcceleration(Segment current, List<Segment> list)
        {
            if (list.Count == 0)
                return true;

            double lastSpeed = list.Last().Speed;
            TimeSpan deltaTime = current.EndTime - current.BeginTime;
            double acceleration = 2 * (current.Speed - lastSpeed) / deltaTime.TotalSeconds;
            return Math.Abs(acceleration) <= _options.MaxAcceleration;
        }
    }
}
