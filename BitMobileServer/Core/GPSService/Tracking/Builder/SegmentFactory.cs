using System;
using System.Collections.Generic;

namespace GPSService.Tracking.Builder
{
    class SegmentFactory
    {
        public SegmentFactory(int initialIndex, int segmentsCount)
        {
            SegmentsCount = segmentsCount;
            InitialIndex = initialIndex;
        }

        public Segment Segment { get; private set; }

        #region Linear approximation

        public int InitialIndex { get; private set; }

        public int SegmentsCount { get; private set; }

        public double ApproximationDevialtion { get; private set; }

        public void BuildApproximation(List<Segment> buffer, TrackingOptions options)
        {
            Segment first = buffer[InitialIndex];
            Segment last = buffer[InitialIndex + SegmentsCount - 1];

            double averageTime = 0;
            double averageLatitude = 0;
            double averageLongitude = 0;

            for (int i = InitialIndex; i < InitialIndex + SegmentsCount; i++)
            {
                Segment trackSegment = buffer[i];

                averageTime += trackSegment.BeginTime.ToOADate();
                averageLatitude += trackSegment.BeginLatitude;
                averageLongitude += trackSegment.BeginLongitude;

                averageTime += trackSegment.EndTime.ToOADate();
                averageLatitude += trackSegment.EndLatitude;
                averageLongitude += trackSegment.EndLongitude;
            }
            averageTime /= SegmentsCount * 2;
            averageLatitude /= SegmentsCount * 2;
            averageLongitude /= SegmentsCount * 2;

            double sumOfDeviationsInLatitude = 0;
            double sumOfDeviationsInLongitude = 0;
            double sumOfSquaresDeviationsInTime = 0;
            for (int i = InitialIndex; i < InitialIndex + SegmentsCount; i++)
            {
                Segment trackSegment = buffer[i];

                double deviationByTime = trackSegment.BeginTime.ToOADate() - averageTime;
                sumOfSquaresDeviationsInTime += deviationByTime * deviationByTime;
                sumOfDeviationsInLatitude += deviationByTime * (trackSegment.BeginLatitude - averageLatitude);
                sumOfDeviationsInLongitude += deviationByTime * (trackSegment.BeginLongitude - averageLongitude);

                deviationByTime = trackSegment.EndTime.ToOADate() - averageTime;
                sumOfSquaresDeviationsInTime += deviationByTime * deviationByTime;
                sumOfDeviationsInLatitude += deviationByTime * (trackSegment.EndLatitude - averageLatitude);
                sumOfDeviationsInLongitude += deviationByTime * (trackSegment.EndLongitude - averageLongitude);
            }

            double lat1 = sumOfDeviationsInLatitude / sumOfSquaresDeviationsInTime;
            double lat0 = averageLatitude - lat1 * averageTime;

            double lon1 = sumOfDeviationsInLongitude / sumOfSquaresDeviationsInTime;
            double lon0 = averageLongitude - lon1 * averageTime;

            ApproximationDevialtion = 0;
            for (int i = InitialIndex; i < InitialIndex + SegmentsCount; i++)
            {
                Segment trackSegment = buffer[i];

                double deviationByLatitude = trackSegment.BeginLatitude - (lat0 + lat1 * trackSegment.BeginTime.ToOADate());
                ApproximationDevialtion += deviationByLatitude * deviationByLatitude;

                double deviationByLongitude = trackSegment.BeginLongitude - (lon0 + lon1 * trackSegment.BeginTime.ToOADate());
                ApproximationDevialtion += deviationByLongitude * deviationByLongitude;

                deviationByLatitude = trackSegment.EndLatitude - (lat0 + lat1 * trackSegment.EndTime.ToOADate());
                ApproximationDevialtion += deviationByLatitude * deviationByLatitude;

                deviationByLongitude = trackSegment.EndLongitude - (lon0 + lon1 * trackSegment.EndTime.ToOADate());
                ApproximationDevialtion += deviationByLongitude * deviationByLongitude;
            }

            ApproximationDevialtion *= GetErrorCorrection(buffer, first.BeginTime, last.EndTime);

            var result = new Segment(first.BeginTime
                , first.BeginLatitude
                , first.BeginLongitude
                , last.EndTime
                , last.EndLatitude
                , last.EndLongitude
                , Math.Min(first.SatellitesCount, last.SatellitesCount)
                , options);

            Segment = result;
        }

        double GetErrorCorrection(List<Segment> buffer, DateTime beginTime, DateTime endTime)
        {
            double minLatitude = double.PositiveInfinity;
            double maxLatitude = double.NegativeInfinity;
            double minLongitude = double.PositiveInfinity;
            double maxLongitude = double.NegativeInfinity;

            for (int i = InitialIndex; i < InitialIndex + SegmentsCount; i++)
            {
                Segment trackSegment = buffer[i];

                minLatitude = Math.Min(minLatitude, trackSegment.EndLatitude);
                maxLatitude = Math.Max(maxLatitude, trackSegment.EndLatitude);
                minLongitude = Math.Min(minLongitude, trackSegment.EndLongitude);
                maxLongitude = Math.Max(maxLongitude, trackSegment.EndLongitude);
            }

            double distance = EllipsoidWGS84.CalcDistance(minLatitude, minLongitude, maxLatitude, maxLongitude);
            double speed = distance / (endTime - beginTime).TotalSeconds;

            if (speed > 3)
                speed = 3;

            return speed / 3;
        }

        #endregion
    }
}
