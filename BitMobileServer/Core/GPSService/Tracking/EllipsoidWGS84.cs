using System;

namespace GPSService.Tracking
{
    // ReSharper disable once InconsistentNaming
    static class EllipsoidWGS84
    {
        const double Pi = 3.1415926535897932384626433832795;
        const double DegreesToRadians = Pi / 180.0;
        const double Alpha = 1.0 / 298.25722356;
        const double A = 6378137;

        public static double CalcDistance(Segment track)
        {
            return CalcDistance(track.BeginLatitude
                , track.BeginLongitude
                , track.EndLatitude
                , track.EndLongitude);
        }

        /// <summary>
        /// Calc distance between two points in meters
        /// </summary>
        public static double CalcDistance(double lat1, double lon1, double lat2, double lon2)
        {
            //Используется метод Андуайе

            //sigma1 = acos(sin(B1) * sin(B2) + cos(B1) * cos(B2) * cos(L2 - L1));
            //M = (sigma1 - 3.0 * sin(sigma1)) / (1 + cos(sigma1));
            //N = (sigma1 + 3 * sin(sigma1)) / (1 - cos(sigma1));
            //U = sin(B1) + sin(B2);
            //U = U * U;
            //V = sin(B1) - sin(B2);
            //V = V * V;

            //dsigma = -0.25 * alpha * (M * U + N * V);
            //s = a * (sigma1 + dsigma);

            double result = 0;

            double latInRad1 = lat1 * DegreesToRadians;
            double lonInRad1 = lon1 * DegreesToRadians;
            double latInRad2 = lat2 * DegreesToRadians;
            double lonInRad2 = lon2 * DegreesToRadians;

            double sinLat1 = Math.Sin(latInRad1);
            double sinLat2 = Math.Sin(latInRad2);
            double cosLat1 = Math.Cos(latInRad1);
            double cosLat2 = Math.Cos(latInRad2);
            double cosDeltaLon = Math.Cos(lonInRad2 - lonInRad1);
            double cosSigma1 = sinLat1 * sinLat2 + cosLat1 * cosLat2 * cosDeltaLon;

            //Погрешность может привести к выходу за допустимый диапазон значений.
            if (cosSigma1 > -1
                && cosSigma1 < 1)
            {
                double sigma1 = Math.Acos(cosSigma1);
                double sinSigma1 = Math.Sin(sigma1);

                double m = (sigma1 - 3.0 * sinSigma1) / (1.0 + cosSigma1);
                double n = (sigma1 + 3.0 * sinSigma1) / (1.0 - cosSigma1);
                double u = sinLat1 + sinLat2;
                u = u * u;
                double v = sinLat1 - sinLat2;
                v = v * v;

                double dSigma = -0.25 * Alpha * (m * u + n * v);

                result = A * (sigma1 + dSigma);
            }

            return result;
        }
    }
}
