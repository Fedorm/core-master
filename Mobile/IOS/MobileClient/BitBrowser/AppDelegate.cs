using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using BitMobile.Controls;
using BitMobile.Utilities.Exceptions;
using BitMobile.Utilities.LogManager;
using BitMobile.Utilities.Translator;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Xml;

namespace BitMobile.IOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		const string LAST_ERROR_FILE = "lasterror.txt";
		// class-level declarations
		UIWindow window;
		NavigationController rootController;
		Settings _settings;
		CustomExceptionHandler _exceptionHandler;
		ApplicationContext _context;

		public ApplicationContext Context {
			get {
				return _context;
			}
		}

		public NavigationController RootController {
			get {
				return rootController;
			}
		}

		public delegate void NSUncaughtExceptionHandler (IntPtr exception);

		[DllImport ("/System/Library/Frameworks/Foundation.framework/Foundation")]
		extern static void NSSetUncaughtExceptionHandler (IntPtr handler);

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			AppDomain.CurrentDomain.UnhandledException += OnException;

			UIApplication.SharedApplication.SetStatusBarHidden (true, false);

			UIApplication.SharedApplication.ApplicationSupportsShakeToEdit = true;

			window = new UIWindow (UIScreen.MainScreen.Bounds);	
			rootController = new NavigationController ();
			window.RootViewController = rootController;
			window.MakeKeyAndVisible ();

			BeginInvokeOnMainThread (() => {
				InitApplication ();
			});

			return true;
		}

		private void InitApplication ()
		{
			_settings = new Settings ();
			_settings.ReadSettings ();

			D.Init (_settings.Language);

			_exceptionHandler = new ExceptionHandler (_settings, this);

			HandleLastError (StartApplication);
		}

		private void HandleLastError (Action nextStep)
		{
			string path = FileErrorPath ();
			if (File.Exists (path)) {
				using (FileStream stream = new FileStream (path, FileMode.Open)) {
					object report = null;

					XmlSerializer serializer = new XmlSerializer (typeof(Log));
					try {
						report = serializer.Deserialize(stream);
					} catch (XmlException) {
						stream.Position = 0;
						using (StreamReader reader = new StreamReader (stream)) 
							report = reader.ReadToEnd ();
					}

					_exceptionHandler.Handle (report, nextStep);
				}
			} else
				nextStep ();
		}

		void StartApplication ()
		{
			File.Delete (FileErrorPath ());
			
			_context = new ApplicationContext (rootController, _settings, _exceptionHandler);
			_context.Start (_settings.ClearCacheOnStart, _settings.ClearCacheOnStart);
		}

		private void OnException (object sender, UnhandledExceptionEventArgs e)
		{
			string report = e.ExceptionObject != null ? e.ExceptionObject.ToString () : "Exception object is null!";

			try {
				using (FileStream stream = new FileStream (FileErrorPath (), FileMode.Create)) {
					Log log = _exceptionHandler.GetLog (true, report);
					XmlSerializer serializer = new XmlSerializer (typeof(Log));
					serializer.Serialize (stream, log);
				}
			} catch (Exception ex) {
				string logReport = "Logger error: " + ex.ToString ();
				logReport += Environment.NewLine;
				logReport += report;

				File.WriteAllText (FileErrorPath (), logReport);
			}

		}

		public string FileErrorPath ()
		{
			return String.Format (@"{0}/{1}", Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), LAST_ERROR_FILE);
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations (UIApplication application, UIWindow forWindow)
		{
			return UIInterfaceOrientationMask.Portrait;
		}

		public override void FinishedLaunching (UIApplication application)
		{
			((GPSTracker)_context.LocationTracker).RestoreMonitoring ();
		}

		public override void DidEnterBackground (UIApplication application)
		{
			UIApplication.SharedApplication.KeyWindow.Subviews.Last ().EndEditing (true);
		}
	}
}

