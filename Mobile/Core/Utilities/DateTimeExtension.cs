using System;

namespace BitMobile.Utilities
{
    public static class DateTimeExtension
    {
        public static readonly DateTime UnixStartTime = new DateTime(1970, 1, 1);

        public static double ToDouble(this DateTime dateTime)
        {
            var unixStartTime = UnixStartTime;

            return (dateTime - unixStartTime).TotalMilliseconds;
        }

        public static DateTime ToDateTime(this long ticks)
        {
            return UnixStartTime + TimeSpan.FromMilliseconds(ticks);
        }
    }
}