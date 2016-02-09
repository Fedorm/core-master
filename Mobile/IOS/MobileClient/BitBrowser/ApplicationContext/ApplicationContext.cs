using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Synchronization.ClientServices.IsolatedStorage;
using MonoTouch.UIKit;
using BitMobile.BusinessProcess;
using BitMobile.Controls;
using BitMobile.ValueStack;
using BitMobile.Factory;
using BitMobile.Utilities.Exceptions;
using System.Net.NetworkInformation;
using BitMobile.DataAccessLayer;
using BitMobile.Common;
using BitMobile.Utilities.Translator;
using System.Threading;
using System.Globalization;
using System.IO;
using MonoTouch.Foundation;
using BitMobile.Utilities.IO;
using System.Drawing;
using MonoTouch.CoreGraphics;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Synchronization.ClientServices;
using BitMobile.DbEngine;
using BitMobile.SyncLibrary.BitMobile;
using System.Xml;
using BitMobile.Common.Entites;
using System.Net;

namespace BitMobile.IOS
{
	public class ApplicationContext : BitMobile.Application.IApplicationContext
	{
		public static bool Busy { get; private set; }

		private LogonController _logonController;
		private ProgressController _progressController;
		private NavigationController _controller;
		private BitMobile.ValueStack.CommonData _commonData;
		private BusinessProcess.BusinessProcess _businessProcess;
		Configuration.Configuration _configuration;
		Func<bool> _back;
		Func<bool> _forward;
		CustomExceptionHandler _exceptionHandler;
		Queue<System.Action> _executionQueue = new Queue<System.Action> ();

		public ApplicationContext (NavigationController controller, BitMobile.Application.ApplicationSettings settings, CustomExceptionHandler exceptionHandler)
		{
			GlobalVariables = new Dictionary<string, object> ();

			BitMobile.Application.ApplicationContext.InitContext (this);

			this.Settings = settings;
			this._controller = controller;
			this._exceptionHandler = exceptionHandler;

			LocationProvider = new GPSProvider ();
			LocationTracker = new GPSTracker ();
			GalleryProvider = new GalleryProvider (controller, this);
			CameraProvider = new CameraProvider (controller, this);
			DialogProvider = new DialogProvider (this);
			ClipboardProvider = new ClipboardProvider ();

		}

		public static Version OSVersion {
			get {
				Version result;
				if (!Version.TryParse (UIDevice.CurrentDevice.SystemVersion, out result))
					result = new Version ();				
				return result;
			}
		}

		public bool SubscribeEvent (string name, Func<bool> action)
		{
			switch (name) {
			case "Back":
				_back += action;
				return true;
			case "Forward":
				_forward += action;
				return true;
			}

			return false;
		}

		public NavigationController MainController {
			get {
				return _controller;
			}
		}

		public UIView GetFirstResponder ()
		{
			var screen = _controller.TopViewController as ScreenController;
			if (screen != null) {
				return screen.GetFirstResponder ();
			}
			throw new Exception ("TopViewController isn't ScreenController");
		}

		public IDictionary<string, string> GetDeviceInfo ()
		{
			Dictionary<string, string> result = new Dictionary<string, string> ();

			result.Add ("deviceId", UniqueID);

			return result;
		}

		public void Start (bool clearCache, bool logout)
		{
			if (Settings.DevelopModeEnabled)
				InitConsole ();

			LoadApplication (clearCache);

			if (Settings.DevelopModeEnabled)
				InitConsole (Database.Current);
		}

		#region IApplicationContext implementation

		public BitMobile.ValueStack.ValueStack ValueStack { get; private set; }

		public IDictionary<string, object> GlobalVariables { get; private set; }

		public Workflow Workflow {
			get { return _businessProcess != null ? _businessProcess.Workflow : null; }
		}

		public DAL DAL { get; private set; }

		public ScreenData CurrentScreen { get; private set; }

		public ILocationProvider LocationProvider { get; private set; }

		public Tracker LocationTracker { get; private set; }

		public BitMobile.Application.ApplicationSettings Settings { get; private set; }

		public IGalleryProvider GalleryProvider { get ; private set; }

		public ICameraProvider CameraProvider { get ; private set; }

		public IDialogProvider DialogProvider { get ; private set; }

		public IClipboardProvider ClipboardProvider { get; private set; }

		public bool OpenScreen (String screenName
			, string controllerName, Dictionary<String
			, object> parameters = null
			, bool isBackCommand = false
			, bool isRefresh = false)
		{
			try {

				Busy = true;

				_back = null;
				_forward = null;

				ValueStack = new ValueStack.ValueStack (_exceptionHandler);
				ValueStack.Push ("common", _commonData);
				ValueStack.Push ("context", this);
				ValueStack.Push ("dao", DAL.DAO);

				foreach (var variable in GlobalVariables)
					ValueStack.Push (variable.Key, variable.Value);

				if (parameters != null) {
					foreach (KeyValuePair<String, object> item in parameters) {
						ValueStack.Push (item.Key, item.Value);
					}
				}

				Controllers.ScreenController newController = 
					ControllerFactory.CreateInstance ().CreateController<Controllers.ScreenController> (controllerName);
				ValueStack.Push ("controller", newController);
				screenName = newController.GetView (screenName);

				TabOrderManager.Create (this);

				Screen scr = (Screen)Factory.ScreenFactory.CreateInstance ().CreateScreen<IOSStyleSheet> (screenName, ValueStack, newController);

				if (CurrentScreen != null)
					((IDisposable)CurrentScreen.Screen).Dispose ();
				CurrentScreen = new ScreenData (screenName, controllerName, scr);

				ScreenController viewController = new ScreenController (scr.View);

				if (!isRefresh) {
					_controller.SetViewControllers (new UIViewController[] {
						_controller.TopViewController
					}, false);

					if (!isBackCommand) {
						_controller.PushViewController (viewController, true);
					} else {
						_controller.SetViewControllers (new UIViewController[] {
							viewController,
							_controller.TopViewController
						}, false);
						_controller.PopViewControllerAnimated (true);
					}
				} else {
					_controller.PopViewControllerAnimated (false);
					_controller.PushViewController (viewController, false);
				}
			} catch (Exception ex) {
				HandleException (ex);
			} finally {
				ActionHandler.Busy = false;
				ActionHandlerEx.Busy = false;
				Busy = false;
			}
			return true;
		}

		public void RefreshScreen (Dictionary<String, object> parameters)
		{
			OpenScreen (CurrentScreen.Name, CurrentScreen.ControllerName, parameters, false, true);
		}

		public void InvokeOnMainThread (System.Action action)
		{
			_executionQueue.Enqueue (action);

			MonoTouch.Foundation.NSAction nsa = new MonoTouch.Foundation.NSAction (InvokeOnMainThreadCallback);

			_controller.BeginInvokeOnMainThread (nsa);
		}

		public void InvokeOnMainThreadSync (System.Action action)
		{
			ManualResetEventSlim sync = new ManualResetEventSlim ();
			this.InvokeOnMainThread (() => {
				action ();
				sync.Set ();
			});
			sync.Wait ();
		}

		public void HandleException (Exception e)
		{
			_exceptionHandler.Handle (e);
		}

		public bool Validate (string args)
		{
			bool result;

			args = args.Trim ();
			if (args.ToLower () == BitMobile.ValueStack.ValueStack.VALIDATE_ALL) {
				result = ((IValidatable)CurrentScreen.Screen).Validate ();
			} else {
				result = true;
				foreach (var item in args.Split(';')) {
					IValidatable validatable = ValueStack.Peek (item.Trim ()) as IValidatable;
					if (validatable != null)
						result &= validatable.Validate ();
				}
			}

			return result;
		}

		public string LocalStorage {
			get {
				string root = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "..", "Library");

				string solution = FileSystemProvider.GetSolutionName (new Uri (Settings.BaseUrl));

				return Path.Combine (root, solution, "filesystem", DAL.UserId.ToString ());
			}
		}

		public async void ScanBarcode (Action<object> callback)
		{
			var scanner = new ZXing.Mobile.MobileBarcodeScanner ();
			var result = await scanner.Scan ();

			if (result != null)
				callback (result.Text);
		}

		public void PhoneCall (string number)
		{
			try {
				var url = new MonoTouch.Foundation.NSUrl (string.Format ("tel://{0}", number));
				UIApplication.SharedApplication.OpenUrl (url);	
			} catch {
				
			}
		}

		public void Wait ()
		{
			DAL.Wait ();

			ManualResetEventSlim sync = new ManualResetEventSlim ();
			this.InvokeOnMainThread (() => {
				sync.Set ();
			});
			sync.Wait ();
		}

		public void Exit (bool clearCache)
		{
			if (clearCache) {
				NSUserDefaults.StandardUserDefaults.SetBool (true, IOS.Settings.KeyClearCacheOnStart);
			}

			UIApplication.SharedApplication.PerformSelector (new MonoTouch.ObjCRuntime.Selector ("terminateWithSuccess"), null, 0f);
		}

		public BitMobile.Configuration.Configuration Configuration {
			get {
				return _configuration;
			}
		}

		#endregion

		void LoadApplication (bool clearCache)
		{
			if (String.IsNullOrEmpty (Settings.UserName) || String.IsNullOrEmpty (Settings.Password)) {
				Logon (clearCache);
				return;
			}

			try {
				_backgroundTaskId =	UIApplication.SharedApplication.BeginBackgroundTask (() => {
				});

				if (this.DAL == null) {
					EntityFactory.CreateInstance = Entity.CreateInstance;
					EntityFactory.DbRefFactory = DbRef.CreateInstance;
					EntityFactory.CustomDictionaryFactory = () => new CustomDictionary ();

					XmlDocument document = LoadMetadata ();
					Uri uri = new Uri (Settings.Url);

					//to create db here
					BitMobile.DbEngine.Database.Init (Settings.BaseUrl);
					IsolatedStorageOfflineContext context = new OfflineContext (document, uri.Host, uri);

					var configuration = document.DocumentElement;
					Settings.ConfigName = configuration.Attributes ["Name"].Value;
					Settings.ConfigVersion = configuration.Attributes ["Version"].Value;
					Settings.WriteSettings();

					IDictionary<string, string> deviceInfo = GetDeviceInfo ();

					this.DAL = new DAL (context
						, Settings.Application
						, Settings.Language
						, Settings.UserName
						, Settings.Password
						, Settings.ConfigName
						, Settings.ConfigVersion
						, deviceInfo
						, UpdateLoadStatus
						, CacheRequestFactory);

					this._commonData = new BitMobile.ValueStack.CommonData ();

					this.Settings.ConfigName = DAL.ConfigName;
					this.Settings.ConfigVersion = DAL.ConfigVersion;
				}

				LogonComplete (clearCache);

			} catch (CustomException ex) {
				Logon (clearCache, ex.FriendlyMessage);
			} catch (UriFormatException) {
				Logon (clearCache, D.INVALID_ADDRESS);
			}
		}

		void InitConsole (Database db = null)
		{
			if (ControllerFactory.Debugger == null)
				ControllerFactory.Debugger = Debugger.Debugger.CreateInstance (Settings.WaitDebuggerEnabled);
			else
				((IDatabaseAware)ControllerFactory.Debugger).SetDatabase (db);
		}

		XmlDocument LoadMetadata ()
		{
			string path = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), Settings.Url.GetHashCode ().ToString ());

			bool fileExists = File.Exists (path);

			if (Settings.ClearCacheOnStart || !fileExists) {
				string uri = Settings.Url + "GetClientMetadata";

				WebRequest request = WebRequest.Create (uri);
				string credentials = Convert.ToBase64String (Encoding.ASCII.GetBytes (Settings.UserName + ":" + Settings.Password));
				request.Headers.Add ("Autorization", "Basic " + credentials);

				using (WebResponse resp = request.GetResponse ())
				using (Stream responseStream = resp.GetResponseStream ()) {
					if (fileExists)
						File.Delete (path);

					using (FileStream fileStream = new FileStream (path, FileMode.Create))
						responseStream.CopyTo (fileStream);					
				}				
			}

			using (FileStream stream = new FileStream (path, FileMode.Open)) {
				var doc = new XmlDocument ();
				doc.Load (stream);
				return doc;
			}
		}

		CacheRequestHandler CacheRequestFactory (Uri serviceUri, CacheControllerBehavior behaviors, AsyncWorkerManager manager)
		{
			if (OSVersion.Major < 7 || Settings.BackgoundLoadDisabled) {
				return new HttpCacheRequestHandler (serviceUri, behaviors, manager);
			} else {
				return new NSUrlCacheRequestHandler (serviceUri, behaviors, manager);
			}
		}

		void UpdateLoadStatus (int total, int processed)
		{
			if (total != 0 && _progressController != null) {
				_controller.BeginInvokeOnMainThread (() => {
					_progressController.UpdateStatus (total, processed);
				});
			}
		}

		BusinessProcess.BusinessProcess LoadBusinessProcess ()
		{
			ValueStack = new ValueStack.ValueStack (_exceptionHandler);
			ValueStack.Push ("context", this);
			ValueStack.Push ("isTablet", UIDevice.CurrentDevice.Model.Contains ("iPad"));

			_configuration = ConfigurationFactory.CreateInstance ().CreateConfiguration (ValueStack);

			return BusinessProcessFactory.CreateInstance ().CreateBusinessProcess (_configuration.BusinessProcess.File, ValueStack);
		}

		public static string UniqueID {
			get {
				var query = new MonoTouch.Security.SecRecord (MonoTouch.Security.SecKind.GenericPassword);
				query.Service = MonoTouch.Foundation.NSBundle.MainBundle.BundleIdentifier;
				query.Account = "BitMobile";

				MonoTouch.Foundation.NSData uniqueId = MonoTouch.Security.SecKeyChain.QueryAsData (query);
				if (uniqueId == null) {
					query.ValueData = MonoTouch.Foundation.NSData.FromString (System.Guid.NewGuid ().ToString ());
					var err = MonoTouch.Security.SecKeyChain.Add (query);
					if (err != MonoTouch.Security.SecStatusCode.Success && err != MonoTouch.Security.SecStatusCode.DuplicateItem)
						return "[Cannot_store_Unique_ID]";

					return query.ValueData.ToString ();
				} else {
					return uniqueId.ToString ();
				}
			}
		}

		void Logon (bool clearCache, String message = null)
		{
			if (_logonController == null)
				_logonController = new LogonController (clearCache, Settings, Start, message);
			else
				_logonController.UpdateMessage (message);
			_controller.SetViewControllers (new UIViewController[] { _logonController }, false);
		}

		int _backgroundTaskId;

		void LogonComplete (bool clearCache)
		{
			Sync (clearCache);
			CreateStatusView ();
		}

		void CreateStatusView ()
		{
			if (_progressController == null)
				_progressController = new ProgressController (Settings);
			_controller.SetViewControllers (new UIViewController[] { _progressController }, false);
		}

		void Sync (bool clearCache)
		{
			DAL.UpdateCredentials (Settings.UserName, Settings.Password);
			DAL.LoadSolution (clearCache, this.Settings.SyncOnStart, LoadComplete);
		}

		void LoadComplete (object sender, SyncEventArgs args)
		{
			if (args.OK)
				_controller.BeginInvokeOnMainThread (() => {
					OpenStartScreen (true);
					UIApplication.SharedApplication.EndBackgroundTask (_backgroundTaskId);
				});
			else {

				CustomException ce = args.Exception as CustomException;
				if (ce != null) {
					string msg = ce.FriendlyMessage;
					Logon (Settings.ClearCacheOnStart, msg);
				} else
					HandleException (args.Exception);
				UIApplication.SharedApplication.EndBackgroundTask (_backgroundTaskId);
			}
		}

		void OpenStartScreen (bool inSync)
		{
			this._commonData.UserId = DAL.UserId;
			this._businessProcess = LoadBusinessProcess ();

			bool success = this._businessProcess != null;
			if (!success) {
				if (!inSync)
					DAL.RefreshAsync (LoadComplete);
				else
					throw new Exception ("Couldn't load context");
			} else {

				if (Settings.DevelopModeEnabled) {
					ControllerFactory.Debugger = BitMobile.Debugger.Debugger.CreateInstance (Settings.WaitDebuggerEnabled);
					(ControllerFactory.Debugger as BitMobile.DbEngine.IDatabaseAware).SetDatabase (BitMobile.DbEngine.Database.Current);
				}

				this._businessProcess.Start (this);
			}

			if (_logonController != null) {
				_logonController.Dispose ();
				_logonController = null;
			}
			if (_progressController != null) {
				_progressController.Dispose ();
				_progressController = null;
			}
		}

		void InvokeOnMainThreadCallback ()
		{
			System.Action action = _executionQueue.Dequeue ();

			action ();
		}
	}
}
