using System.Collections.Generic;

namespace GPSService.Tracking.Builder
{
    public class TrackBuilder
    {
        private readonly TrackingOptions _options;
        private readonly TrackingFilter _filter;

        public TrackBuilder(TrackingOptions options)
        {
            _options = options;
            _filter = new TrackingFilter(options);
        }

        public List<Segment> Build(IList<Point> monitoringData)
        {
            if (monitoringData == null || monitoringData.Count == 0)
                return new List<Segment>();

            List<Segment> track = null;
            for (int i = 0; i < monitoringData.Count; i++)
            {
                bool built = BuildTrack(monitoringData, i, out track);
                if (built)
                    break;
                track = null;
            }

            if (track == null)
                return new List<Segment>();
            return track;
        }

        private bool BuildTrack(IList<Point> monitoringData, int index, out List<Segment> track)
        {
            track = new List<Segment>();
            var approximator = new Approximator();
            Point last = monitoringData[index];
            int errors = 0;
            int startCount = monitoringData.Count > _options.StartTrackCount
                        ? _options.StartTrackCount
                        : monitoringData.Count;

            for (int i = index + 1; i < monitoringData.Count; i++)
            {
                var current = monitoringData[i];
                var segment = new Segment(last, current, _options);

                if (_filter.ValidateSegment(segment, track))
                {
                    var newSegment = approximator.Execute(segment, _options);

                    AttachToTrack(track, newSegment);

                    last = current;
                }
                else
                {
                    errors++;
                    if (i < startCount && errors > startCount * _options.MaxErrorsPercentage)
                        return false;
                }
            }

            return true;
        }


        private static void AttachToTrack(List<Segment> track, Segment testSegment)
        {
            if (track.Count > 0 && track[track.Count - 1].BeginTime == testSegment.BeginTime)
                track[track.Count - 1] = testSegment;
            else
                track.Add(testSegment);
        }

    }
}
