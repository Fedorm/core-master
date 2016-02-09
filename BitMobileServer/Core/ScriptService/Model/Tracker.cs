using System;
using System.Collections.Generic;
using GPSService.Tracking;
using GPSService.Tracking.Builder;

namespace ScriptService.Model
{
    class Tracker
    {
        public Tracker()
        {
            Options = new TrackingOptions();
        }

        public TrackingOptions Options { get; private set; }

        public List<Segment> Build(IDbRecordset recordset)
        {
            var points = new List<Point>();
            while (recordset.Read())
            {
                var dateTime = (DateTime)recordset["EndTime"];
                var latitude = (double)Convert.ChangeType(recordset["Latitude"], typeof(double));
                var longitude = (double)Convert.ChangeType(recordset["Longitude"], typeof(double));
                var satellitesCount = (int)recordset["SatellitesCount"];

                var point = new Point(dateTime, latitude, longitude, satellitesCount);
                points.Add(point);
            }

            var builder = new TrackBuilder(Options);
            return builder.Build(points);
        }
    }
}
