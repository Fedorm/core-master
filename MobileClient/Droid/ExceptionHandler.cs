using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Android.Net;
using BitMobile.Application;
using BitMobile.Application.Exceptions;
using BitMobile.Application.Log;
using BitMobile.Application.Translator;
using System;
using BitMobile.Common.Log;
using Android.Content;
using BitMobile.Droid.Application;
using BitMobile.Droid.Providers;
using Java.Util;
using TimeZone = Java.Util.TimeZone;

namespace BitMobile.Droid
{
    class ExceptionHandler : CustomExceptionHandler
    {
        readonly Settings _settings;
        readonly BaseScreen _screen;
        private readonly AndroidApplicationContext _context;

        public ExceptionHandler(Settings settings, BaseScreen screen, AndroidApplicationContext context)
        {
            _settings = settings;
            _screen = screen;
            _context = context;
        }

        protected override void PrepareException(Exception e, Action<Exception> next)
        {
            if (ApplicationContext.Current.Settings.DevelopModeEnabled)
            {
                string title = D.APP_HAS_BEEN_INTERRUPTED;
                var ce = e as CustomException;
                if (ce != null)
                    title = ce.FriendlyMessage;

                ShowDialog(title, e.Message, index => next(e), D.OK, null);
            }
            else
                next(e);
        }

        protected override IDictionary<string, string> GetNativeInfo()
        {
            var result = new Dictionary<string, string>();
            if (!BitBrowserApp.Current.Crashing)
            {
                using (var cm = (ConnectivityManager)_screen.GetSystemService(Context.ConnectivityService))
                {
                    string connection;
                    NetworkInfo info = cm.ActiveNetworkInfo;
                    if (info != null && info.IsConnectedOrConnecting)
                        connection = info.Type.ToString();
                    else
                        connection = "None";
                    result.Add("Connection", connection);
                }

                result.Add("Language", Locale.Default.Language);
                result.Add("TimeZone", TimeZone.Default.DisplayName);

                result.Add("Board", Android.OS.Build.Board);
                result.Add("Bootloader", Android.OS.Build.Bootloader);
                result.Add("Brand", Android.OS.Build.Brand);
                result.Add("CpuAbi", Android.OS.Build.CpuAbi);
                result.Add("CpuAbi2", Android.OS.Build.CpuAbi2);
                result.Add("Device", Android.OS.Build.Device);
                result.Add("Display", Android.OS.Build.Display);
                result.Add("Fingerprint", Android.OS.Build.Fingerprint);
                result.Add("Hardware", Android.OS.Build.Hardware);
                result.Add("Host", Android.OS.Build.Host);
                result.Add("Id", Android.OS.Build.Id);
                result.Add("Manufacturer", Android.OS.Build.Manufacturer);
                result.Add("Model", Android.OS.Build.Model);
                result.Add("Product", Android.OS.Build.Product);
                result.Add("Radio", Android.OS.Build.Radio);
                result.Add("Tags", Android.OS.Build.Tags);
                result.Add("Time", Android.OS.Build.Time.ToString(CultureInfo.InvariantCulture));
                result.Add("Type", Android.OS.Build.Type);
                result.Add("User", Android.OS.Build.User);
                result.Add("VERSION.Codename", Android.OS.Build.VERSION.Codename);
                result.Add("VERSION.Incremental", Android.OS.Build.VERSION.Incremental);
                result.Add("VERSION.Release", Android.OS.Build.VERSION.Release);
                result.Add("VERSION.Sdk", Android.OS.Build.VERSION.Sdk);
            }
            return result;
        }

        protected override void ShutDownApplication()
        {
            _screen.ShutDown();
        }

        protected async override void ShowDialog(string title, string message, Action<int> onClick, string cancelButtonTitle, params string[] otherButtons)
        {
            string negativeButtonTitle = null;
            if (otherButtons != null && otherButtons.Length > 0)
                negativeButtonTitle = otherButtons[0];

            DialogProvider.DialogButton result = await _context.DialogProviderInternal.ShowDialog(title, message
                , cancelButtonTitle, negativeButtonTitle);

            onClick((int)result);
        }
    }
}
