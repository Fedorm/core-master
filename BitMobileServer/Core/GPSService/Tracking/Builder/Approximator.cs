using System.Collections.Generic;

namespace GPSService.Tracking.Builder
{
    class Approximator
    {
        private const double Tolerance = 0.0000005 / 2.0;

        private readonly List<Segment> _buffer = new List<Segment>();

        public Segment Execute(Segment segment, TrackingOptions options)
        {
            if (segment.IsBreak)
            {
                _buffer.Clear();
                return segment;
            }

            _buffer.Add(segment);

            var entireFactory = new SegmentFactory(0, _buffer.Count);
            entireFactory.BuildApproximation(_buffer, options);

            if (entireFactory.ApproximationDevialtion < Tolerance || _buffer.Count < 3)
                return entireFactory.Segment;

            SegmentFactory resultFactory = null;
            double resultError = 0;

            for (int i = _buffer.Count - 1; i >= 1; i--)
            {
                var mainFactory = new SegmentFactory(0, i);
                mainFactory.BuildApproximation(_buffer, options);

                var extraFactory = new SegmentFactory(i, _buffer.Count - i);
                extraFactory.BuildApproximation(_buffer, options);

                var error = mainFactory.ApproximationDevialtion
                    + extraFactory.ApproximationDevialtion;

                if (resultFactory == null)
                {
                    resultFactory = mainFactory;
                    resultError = error;
                }
                else if (error < resultError)
                {
                    resultFactory = mainFactory;
                    resultError = error;
                }

            }

            // ReSharper disable once PossibleNullReferenceException
            _buffer.RemoveRange(resultFactory.InitialIndex, resultFactory.SegmentsCount);

            return resultFactory.Segment;
        }


    }
}
