using System;
using System.Collections.Generic;
using BitMobile.Application;
using BitMobile.Application.Log;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
    public class ExceptionHandler : CustomExceptionHandler
    {
        private AppDelegate _application;
        private ApplicationSettings _settings;

        public ExceptionHandler(ApplicationSettings settings, AppDelegate application)
        {
            _settings = settings;
            _application = application;
        }

        #region implemented abstract members of CustomExceptionHandler

        protected override void PrepareException(Exception e, Action<Exception> next)
        {
            // TODO: Show message with information for developers in debug mode
            next(e);
        }

        protected override IDictionary<string, string> GetNativeInfo()
        {
            var result = new Dictionary<string, string>();

            result.Add("Connection", Reachability.RemoteHostStatus().ToString());
            result.Add("Language", NSLocale.PreferredLanguages[0]);
            result.Add("TimeZone", NSTimeZone.LocalTimeZone.Name);

            result.Add("Description", UIDevice.CurrentDevice.Description);
            result.Add("IdentifierForVendor", UIDevice.CurrentDevice.IdentifierForVendor.ToString());
            result.Add("Model", UIDevice.CurrentDevice.Model);
            result.Add("Name", UIDevice.CurrentDevice.Name);
            result.Add("SystemName", UIDevice.CurrentDevice.SystemName);
            result.Add("SystemVersion", UIDevice.CurrentDevice.SystemVersion);

            return result;
        }

        protected override void ShutDownApplication()
        {
            AppDelegate.ShutDownApplication();
        }

        protected override void ShowDialog(string title, string message, Action<int> onClick, string cancelButtonTitle,
            params string[] otherButtons)
        {
            var alert = new UIAlertView(title, message, null, cancelButtonTitle, otherButtons);
            alert.Clicked += (sender, e) => onClick(e.ButtonIndex);
            alert.Show();
        }

        #endregion
    }
}