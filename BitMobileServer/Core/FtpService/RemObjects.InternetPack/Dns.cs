/*---------------------------------------------------------------------------
  RemObjects Internet Pack for .NET - Core Library
  (c)opyright RemObjects Software, LLC. 2003-2012. All rights reserved.

  Using this code requires a valid license of the RemObjects Internet Pack
  which can be obtained at http://www.remobjects.com?ip.
---------------------------------------------------------------------------*/

using System;
using System.Net;

namespace RemObjects.InternetPack.Dns
{
    public class DnsLookup
    {
        public static IPAddress ResolveFirst(String hostname)
        {
            IPAddress[] lAddresses = ResolveAll(hostname);
            if (lAddresses.Length == 0)
                throw new DnsResolveException(String.Format("Could not resolve HostName {0}", hostname));

            return lAddresses[0];
        }

        public static IPAddress ResolveRandom(String hostname)
        {
            IPAddress[] lAddresses = ResolveAll(hostname);
            if (lAddresses.Length == 0)
                throw new DnsResolveException(String.Format("Could not resolve hostname {0}", hostname));

            Random lRandom = new Random();
            return lAddresses[lRandom.Next(lAddresses.Length)];
        }

        public static IPAddress[] ResolveAll(String hostname)
        {
            IPAddress lAddress = TryStringAsIPAddress(hostname);
            if (lAddress != null)
                return new IPAddress[] { lAddress };

            IPHostEntry lEntry = System.Net.Dns.GetHostEntry(hostname);
            return lEntry.AddressList;
        }

        public static IPAddress TryStringAsIPAddress(String hostname)
        {
            if (hostname.StartsWith("[")) // ipv6
            {
                try
                {
                    return IPAddress.Parse(hostname);
                }
                catch (FormatException)
                {
                    return null;
                }
            }

            String[] lFields = hostname.Split('.');
            if (lFields.Length != 4)
                return null;

            for (Int32 i = 0; i < 4; i++)
            {
                if (lFields[i].Length > 3)
                    return null;

                for (Int32 j = 0; j < lFields[i].Length; j++)
                    if (lFields[i][j] < '0' && lFields[i][j] > '9')
                        return null;
            }
            try
            {
                return IPAddress.Parse(hostname);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        public static String ReverseResolve(IPAddress address)
        {
            IPHostEntry lEntry = System.Net.Dns.GetHostEntry(address);

            return lEntry.HostName;
        }
    }

#if FULLFRAMEWORK
    [System.Serializable()]
#endif
    public class DnsResolveException : Exception
    {
        public DnsResolveException(String message)
            : base(message)
        {
        }
    }
}