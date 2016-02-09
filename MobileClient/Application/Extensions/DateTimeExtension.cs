using System;

namespace BitMobile.Application.Extensions
{
    public static class DateTimeExtension
    {
        public static readonly DateTime UnixStartTime = new DateTime(1970, 1, 1);

        public static double ToDouble(this DateTime dateTime)
        {
            return (dateTime - UnixStartTime).TotalMilliseconds;
        }

        public static DateTime ToDateTime(this long milliseconds)
        {
            return UnixStartTime + TimeSpan.FromMilliseconds(milliseconds);
        }
    }
}