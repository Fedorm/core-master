﻿using System;
using System.Runtime.InteropServices;
using MonoTouch;

namespace JMABarcodeMT
{
    /// <summary>
    ///     This code source is:
    ///     http://snippets.dzone.com/user/zachgris
    ///     Detail descriptions how to determine what iPhone hardware is:
    ///     http://www.drobnik.com/touch/2009/07/determining-the-hardware-model/
    /// </summary>
    public static class DeviceHardware
    {
        // make sure to add a 'using System.Runtime.InteropServices;' line to your file
        public const string HardwareProperty = "hw.machine";

        private static float _dpi;

        public static string Version
        {
            get
            {
                // get the length of the string that will be returned
                IntPtr pLen = Marshal.AllocHGlobal(sizeof (int));
                sysctlbyname(HardwareProperty, IntPtr.Zero, pLen, IntPtr.Zero, 0);

                int length = Marshal.ReadInt32(pLen);

                // check to see if we got a length
                if (length == 0)
                {
                    Marshal.FreeHGlobal(pLen);
                    return "unknown";
                }

                // get the hardware string
                IntPtr pStr = Marshal.AllocHGlobal(length);
                sysctlbyname(HardwareProperty, pStr, pLen, IntPtr.Zero, 0);

                // convert the native string into a C# string
                string hardwareStr = Marshal.PtrToStringAnsi(pStr);

                // cleanup
                Marshal.FreeHGlobal(pLen);
                Marshal.FreeHGlobal(pStr);

                return hardwareStr;
            }
        }

        public static float Dpi
        {
            get
            {
                if (_dpi == 0f)
                {
                    string hardwareStr = Version;
                    if (hardwareStr == "iPhone1,2" || hardwareStr == "iPhone2,1")
                        _dpi = 163; // iPhone 3
                    else if (hardwareStr.Contains("iPhone"))
                        _dpi = 326; // iPhone 4, iPhone 5
                    else if (hardwareStr == "iPad1,1" || hardwareStr == "iPad2,1" || hardwareStr == "iPad2,2" ||
                             hardwareStr == "iPad2,3" || hardwareStr == "iPad2,4")
                        _dpi = 132; // iPad 1, iPad 2
                    else if (hardwareStr == "iPad2,5" || hardwareStr == "iPad2,6" || hardwareStr == "iPad2,7")
                        _dpi = 163; // iPad Mini
                    else if (hardwareStr.Contains("iPad3") || hardwareStr == "iPad4,1" || hardwareStr == "iPad4,2")
                        _dpi = 264; // iPad 3, iPad 4, iPad Air
                    else if (hardwareStr == "iPad4,4" || hardwareStr == "iPad4,5")
                        _dpi = 326; // iPad Mini Retina
                    else if (hardwareStr == "i386" || hardwareStr == "x86_64")
                        _dpi = 163; // Simulator (because i'm using IPhone simulator)
                    else
                        _dpi = 326; // For the future devices		
                }

                return _dpi;
            }
        }

        [DllImport(Constants.SystemLibrary)]
        internal static extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string property,
            // name of the property             IntPtr output, // output             IntPtr oldLen, // IntPtr.Zero             IntPtr newp, // IntPtr.Zero             uint newlen // 0
            );
    }
}