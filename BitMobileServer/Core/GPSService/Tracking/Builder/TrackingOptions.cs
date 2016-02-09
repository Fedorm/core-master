namespace GPSService.Tracking.Builder
{
    public class TrackingOptions
    {
        private const double DefaultMaxErrorsPercentage = 0.5;
        private double _maxErrorsPercentage;

        public TrackingOptions()
        {
            MaxSpeed = 56; // 200 km/h
            MaxAcceleration = 7; // 0 to 100 in 4s
            MinDistance = 1;
            MinSatellites = 4;
            ParkingInterval = 10;
            ParkingSpeed = 1;
            BreakDistance = 500;
            BreakDuration = 300;
            _maxErrorsPercentage = DefaultMaxErrorsPercentage;
            StartTrackCount = 300;
        }

        public byte MinSatellites { get; set; }


        public double MaxErrorsPercentage
        {
            get { return _maxErrorsPercentage; }
            set
            {
                _maxErrorsPercentage = value > 0 && value <= 1 
                    ? value : DefaultMaxErrorsPercentage;
            }
        }

        public int StartTrackCount { get; set; }

        /// <summary>
        /// m/s
        /// </summary>
        public double MaxSpeed { get; set; }

        /// <summary>
        /// m/s^2
        /// </summary>
        public double MaxAcceleration { get; set; }

        /// <summary>
        /// m
        /// </summary>
        public double MinDistance { get; set; }

        /// <summary>
        /// s
        /// </summary>
        public int ParkingInterval { get; set; }

        /// <summary>
        /// m/s
        /// </summary>
        public int ParkingSpeed { get; set; }

        /// <summary>
        /// m
        /// </summary>
        public int BreakDistance { get; set; }

        /// <summary>
        /// m
        /// </summary>
        public int BreakDuration { get; set; }

    }
}
