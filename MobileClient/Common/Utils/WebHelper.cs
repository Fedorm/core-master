using System;

namespace BitMobile.Common.Utils
{
    public static class WebHelper
    {
        public static string ToCurrentScheme(this string url, bool httpsDisabled)
        {

            var builder = new UriBuilder(url);
            builder.Scheme = httpsDisabled ?  "http" : "https";
            if (httpsDisabled && builder.Port == 443)
            {
                builder.Port = 80;
            }else if (!httpsDisabled && builder.Port == 80)
            {
                builder.Port = 443;
            }
            return builder.ToString();
        }
    }
}