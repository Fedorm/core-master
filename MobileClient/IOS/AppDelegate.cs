using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BitMobile.Application.BusinessProcess;
using BitMobile.Application.Exceptions;
using BitMobile.Application.Log;
using BitMobile.Application.Translator;
using BitMobile.Common.Log;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to
    // application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public delegate void NSUncaughtExceptionHandler(IntPtr exception);

        private const string LastErrorFile = "lasterror.txt";
        private IOSApplicationContext _context;
        private CustomExceptionHandler _exceptionHandler;
        private NavigationController _rootController;
        private Settings _settings;
        private UIWindow _window;
        internal static PushNotificationManager NotificationManager;

        public IOSApplicationContext Context
        {
            get { return _context; }
        }

        public NavigationController RootController
        {
            get { return _rootController; }
        }

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            AppDomain.CurrentDomain.UnhandledException += OnException;

            UIApplication.SharedApplication.SetStatusBarHidden(true, false);

            UIApplication.SharedApplication.ApplicationSupportsShakeToEdit = true;

            _window = new UIWindow(UIScreen.MainScreen.Bounds);
            _rootController = new NavigationController();
            _window.RootViewController = _rootController;
            _window.MakeKeyAndVisible();

            NotificationManager = new PushNotificationManager();
            NotificationManager.InitLocalNotifications(options);

            BeginInvokeOnMainThread(InitApplication);

            return true;
        }

        public static void ShutDownApplication()
        {
            LogManager.Logger.ApplicationClosed();

            UIApplication.SharedApplication.PerformSelector(new Selector("terminateWithSuccess"), null, 0f);
        }

        public static void HandleException(string exeption)
        {
            try
            {
                using (var stream = new FileStream(FileErrorPath(), FileMode.Create))
                    LogManager.Reporter.CreateReport(exeption, ReportType.Crash).Serialize(stream);
            }
            finally
            {
                if (LogManager.Logger != null)
                    LogManager.Logger.Error(exeption, true);
            }
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, UIWindow forWindow)
        {
            return UIInterfaceOrientationMask.Portrait;
        }

        public override void FinishedLaunching(UIApplication application)
        {
            ((GPSTracker)_context.LocationTracker).RestoreMonitoring();
        }

        public override void OnActivated(UIApplication application)
        {
            if (_context != null)
                _context.OnApplicationForeground();

            if (LogManager.Logger != null)
                LogManager.Logger.ApplicationMaximized();

            if (_context != null && _context.Workflow != null)
                BusinessProcessContext.Current.GlobalEventsController.OnApplicationRestore(_context.Workflow.Name);
        }

        public override void OnResignActivation(UIApplication application)
        {
            UIApplication.SharedApplication.KeyWindow.Subviews.Last().EndEditing(true);

            if (_context != null)
                _context.OnApplicationBackground();

            if (LogManager.Logger != null)
                LogManager.Logger.ApplicationMinimized();

            if (_context != null && _context.Workflow != null)
                BusinessProcessContext.Current.GlobalEventsController.OnApplicationBackground(_context.Workflow.Name);
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            NotificationManager.SaveCurrentToken(deviceToken);
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            if (_context != null)
                _context.HandleException(new NonFatalException("Error registering push notifications", error.LocalizedDescription));
        }

        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            NotificationManager.ReceivedRemoteNotification(userInfo);
        }

        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            NotificationManager.ReceivedLocalNotification(notification);
        }

        public override void ReceiveMemoryWarning(UIApplication application)
        {            
            NSUrlCache.SharedCache.RemoveAllCachedResponses();
            GC.Collect();
        }

        private static string FileErrorPath()
        {
            return string.Format(@"{0}/{1}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), LastErrorFile);
        }

        private void InitApplication()
        {
            _settings = new Settings();
            _settings.ReadSettings();

            D.Init(_settings.Language);

            _exceptionHandler = new ExceptionHandler(_settings, this);

            _context = new IOSApplicationContext(this, _rootController, _settings, _exceptionHandler);

			NotificationManager.SetApplicationContext (_context);
            NotificationManager.InitRemoteNotifications();

            HandleLastError(StartApplication);
        }

        private void HandleLastError(Action nextStep)
        {
            string path = FileErrorPath();
            if (File.Exists(path))
            {
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    IReport report = LogManager.Reporter.CreateReport(stream);
                    _exceptionHandler.Handle(report, nextStep);
                }
            }
            else
                nextStep();
        }

        private void StartApplication()
        {
            File.Delete(FileErrorPath());

            _context.Start(_settings.ClearCacheOnStart);
        }

        private void OnException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject != null ? e.ExceptionObject.ToString() : "Exception object is null!");
        }
    }
}