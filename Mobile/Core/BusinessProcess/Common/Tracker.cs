using System;
using BitMobile.DbEngine;
using System.Net;
using System.Text;
using System.IO;
using System.Xml;
using System.Data;

namespace BitMobile.Common
{
    public abstract class Tracker
    {
        Guid _currentId = Guid.Empty;
        double _currentLattitude;
        double _currentLongitude;

        DateTime _sendTime;

        public int DistanceFilter { get; set; }

        public int SendInterval { get; set; }

        public abstract bool StartTracking(bool bestAccuracy, int distance, TimeSpan interval);

        public abstract bool StopTracking();

        protected void OnLocationChanged(LocationEventArgs args)
        {
            double distance = GetDistance(_currentLattitude, _currentLongitude, args.Latitude, args.Longitude);

            if (distance < DistanceFilter)
            {
                string q = string.Format("UPDATE {0} SET EndTime=@p1 WHERE Id=@p2"
                    , Database.LocationsTable);
                Database.Current.ExecuteNonQuery(q, args.Time, _currentId);
            }
            else
            {
				string q = string.Format("INSERT INTO {0} VALUES(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9)"
                    , Database.LocationsTable);

                Guid guid = Guid.NewGuid();
                Database.Current.ExecuteNonQuery(q, guid, args.Latitude, args.Longitude
					, args.Time, args.Time, args.Speed, args.Direction, args.SatellitesCount, args.Altitude);

                _currentId = guid;
                _currentLattitude = args.Latitude;
                _currentLongitude = args.Longitude;

                if ((DateTime.Now - _sendTime).TotalSeconds > SendInterval)
                {
                    SendData();
                    _sendTime = DateTime.Now;
                }
            }
        }

        private void SendData()
        {
            Database db = Database.Current;
            var tbl = db.SelectAsDataTable("data", String.Format(
				"SELECT [BeginTime], [EndTime], [Latitude], [Longitude], [Speed], [Direction], [SatellitesCount], [Altitude] FROM {0}", Database.LocationsTable), new object[] { });
            if (tbl.Rows.Count > 0)
            {
                var ub = new UriBuilder(String.Format(@"{0}/gps/postdata", Application.ApplicationContext.Context.Settings.BaseUrl));

                var req = (HttpWebRequest)WebRequest.Create(ub.Uri);
                req.Method = "POST";
                req.Headers.Add("userid", Application.ApplicationContext.Context.DAL.UserId.ToString());

                try
                {
                    Stream s = req.GetRequestStream();
                    using (var w = new XmlTextWriter(s, Encoding.UTF8))
                    {
                        tbl.WriteXml(w, XmlWriteMode.WriteSchema);
                        w.Flush();
                    }

                    var resp = (HttpWebResponse)req.GetResponse();
                    String result;
                    using (var r = new StreamReader(resp.GetResponseStream()))
                        result = r.ReadToEnd();

                    resp.Close();

                    if (result == "ok")
                    {
                        string q = String.Format("DELETE FROM {0} WHERE Id <> @p1", Database.LocationsTable);
                        Database.Current.ExecuteNonQuery(q, _currentId);
                    }
                }
                catch (Exception) //bugaga !!!
                {
                    Console.WriteLine();
                }
            }
        }


        /// <summary>
        /// Distance in meters
        /// </summary>
        /// <returns></returns>
        static double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            //Haversine formula:
            //a = sin²(Δf/2) + cos f1 ⋅ cos f2 ⋅ sin²(Δl/2)
            //c = 2 ⋅ atan2( √a, √(1−a) )
            //d = R ⋅ c
            //where	f is latitude, l is longitude, R is earth’s radius (mean radius = 6,371km);
            //note that angles need to be in radians to pass to trig functions!

            const double r = 6371;
            double f1 = lat1 * Math.PI / 180;
            double f2 = lat2 * Math.PI / 180;
            double deltaf = f2 - f1;
            double deltal = (lon2 - lon1) * Math.PI / 180;

            double a = Math.Pow(Math.Sin(deltaf / 2), 2)
                + Math.Cos(f1) * Math.Cos(f2) * Math.Pow(Math.Sin(deltal / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double result = r * c;

            return Math.Abs(result * 1000);
        }

        protected class LocationEventArgs : EventArgs
        {
            public LocationEventArgs(double latitude, double longitude, DateTime time
				, double speed, double direction, int satellitesCount, double altitude)
            {
                Speed = speed;
                Latitude = latitude;
                Longitude = longitude;
                Time = time;
                Direction = direction;
                SatellitesCount = satellitesCount;
				Altitude = altitude;
            }

            public double Latitude { get; private set; }

            public double Longitude { get; private set; }

            public DateTime Time { get; private set; }

            public int SatellitesCount { get; private set; }

            public double Speed { get; private set; }

            public double Direction { get; private set; }

			public double Altitude {get; private set; }
        }

    }
}

