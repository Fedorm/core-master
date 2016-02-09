using System;
using System.Runtime.InteropServices;
using MonoTouch;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
    public class Application
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBePrivate.Global
        public delegate void NSUncaughtExceptionHandler(IntPtr exception);

        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        private static extern void NSSetUncaughtExceptionHandler(IntPtr handler);

        // This is the main entry point of the application.
        private static void Main(string[] args)
        {
            try
            {
                NSSetUncaughtExceptionHandler(
                    Marshal.GetFunctionPointerForDelegate(new NSUncaughtExceptionHandler(MyUncaughtExceptionHandler)));

                // if you want to use a different Application Delegate class from "AppDelegate"
                // you can specify it here.
                UIApplication.Main(args, null, "AppDelegate");
            }
            catch (Exception e)
            {
                AppDelegate.HandleException(e.ToString());
                throw;
            }
        }

        [MonoPInvokeCallback(typeof(NSUncaughtExceptionHandler))]
        private static void MyUncaughtExceptionHandler(IntPtr exception)
        {
            var e = new NSException(exception);
            AppDelegate.HandleException(e.ToString());
        }
    }
}