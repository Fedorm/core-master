using System;
using System.Collections.Generic;
using System.Text;
using MonoTouch.UIKit;
using BitMobile.Controls;
using BitMobile.ValueStack;
using System.Net.NetworkInformation;
using BitMobile.Common;
using System.Threading;
using System.Globalization;
using System.IO;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.CoreGraphics;
using System.Threading.Tasks;
using System.Linq;
using System.Xml;
using BitMobile.Common.Entites;
using System.Net;
using BitMobile.Common.Application;
using BitMobile.Common.ValueStack;
using BitMobile.Common.BusinessProcess.WorkingProcess;
using BitMobile.Application.LogManager;
using BitMobile.Common.BusinessProcess.SolutionConfiguration;
using IBusinessProcess = BitMobile.Common.BusinessProcess.WorkingProcess.IBusinessProcess;
using BitMobile.Bulder;
using BitMobile.Common.DataAccessLayer;
using BitMobile.Common.Controls;
using BitMobile.Common.Device.Providers;
using BitMobile.Common.Application.Tracking;
using BitMobile.Application.DbEngine;
using BitMobile.Application.ValueStack;
using BitMobile.Application.BusinessProcess;
using BitMobile.Common.StyleSheet;
using BitMobile.Application.StyleSheet;
using BitMobile.Application.Controls;
using BitMobile.Application.IO;
using BitMobile.Common.SyncLibrary;
using BitMobile.Application.SyncLibrary;
using BitMobile.Application.DataAccessLayer;
using BitMobile.Application.Exceptions;
using BitMobile.Application.Translator;
using BitMobile.Common.DbEngine;

namespace BitMobile.IOS
{
	public class ApplicationContext : IApplicationContext
	{
		public static bool Busy { get; private set; }

		private LogonController _logonController;
		private ProgressController _progressController;
		private NavigationController _controller;
		private ICommonData _commonData;
		private IBusinessProcess _businessProcess;
		private	IConfiguration _configuration;
		private	Func<bool> _back;
		private	Func<bool> _forward;
		CustomExceptionHandler _exceptionHandler;
		Queue<System.Action> _executionQueue = new Queue<System.Action> ();

		public ApplicationContext (NavigationController controller, BitMobile.Application.ApplicationSettings settings, CustomExceptionHandler exceptionHandler)
		{
			GlobalVariables = new Dictionary<string, object> ();

			Settings = settings;
			_controller = controller;
			_exceptionHandler = exceptionHandler;

			LocationProvider = new GPSProvider ();
			LocationTracker = new GPSTracker ();
			GalleryProvider = new GalleryProvider (controller, this);
			CameraProvider = new CameraProvider (controller, this);
			DialogProvider = new DialogProvider (this);
			DisplayProvider = new DisplayProvider ();

			var builder = new SolutionBuilder (this);
			builder.Build ();

			StyleSheetContext.Current.Scale = UIScreen.MainScreen.Scale;
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

		public void Start (bool clearCache)
		{
			if (Settings.DevelopModeEnabled)
				BusinessProcessContext.Current.InitConsole (Settings.WaitDebuggerEnabled);

			LoadApplication (clearCache);

			if (Settings.DevelopModeEnabled)
				BusinessProcessContext.Current.InitConsole (Settings.WaitDebuggerEnabled, DbContext.Current.Database);
		}

		#region IApplicationContext implementation

		public IValueStack ValueStack { get; private set; }

		public IDictionary<string, object> GlobalVariables { get; private set; }

		public IWorkflow Workflow {
			get { return _businessProcess != null ? _businessProcess.Workflow : null; }
		}

		public IDal Dal { get; private set; }

		public IScreenData CurrentScreen { get; private set; }

		public ILocationProvider LocationProvider { get; private set; }

		public ITracker LocationTracker { get; private set; }

		public IApplicationSettings Settings { get; private set; }

		public IGalleryProvider GalleryProvider { get ; private set; }

		public ICameraProvider CameraProvider { get ; private set; }

		public IDialogProvider DialogProvider { get ; private set; }

		public IDisplayProvider DisplayProvider { get ; private set; }

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

				ValueStack = ValueStackContext.Current.CreateValueStack (_exceptionHandler);
				ValueStack.Push ("common", _commonData);
				ValueStack.Push ("context", this);
				ValueStack.Push ("dao", Dal.Dao);

				foreach (var variable in GlobalVariables)
					ValueStack.Push (variable.Key, variable.Value);

				if (parameters != null) {
					foreach (KeyValuePair<String, object> item in parameters) {
						ValueStack.Push (item.Key, item.Value);
					}
				}

				var newController = BusinessProcessContext.Current.CreateScreenController (controllerName);
				ValueStack.Push ("controller", newController);
				screenName = newController.GetView (screenName);

				TabOrderManager.Create (this);

				IStyleSheet styleSheet = StyleSheetContext.Current.CreateStyleSheet ();

				Screen scr = (Screen)BusinessProcessContext.Current.CreateScreenFactory ().CreateScreen (screenName, ValueStack, newController, styleSheet);

				if (CurrentScreen != null)
					((IDisposable)CurrentScreen.Screen).Dispose ();
				CurrentScreen = ControlsContext.Current.CreateScreenData (screenName, controllerName, scr);

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
				ControlsContext.Current.ActionHandlerIsBusy = false;
				ControlsContext.Current.ActionHandlerExIsBusy = false;
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
			if (args.ToLower () == ValueStackConst.ValidateAll) {
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

				string solution = IOContext.Current.GetSolutionName (new Uri (Settings.BaseUrl));

				return Path.Combine (root, solution, "filesystem", Dal.UserId.ToString ());
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
			} catch  {

			}
		}

		public void Wait ()
		{
			Dal.Wait ();

			ManualResetEventSlim sync = new ManualResetEventSlim ();
			this.InvokeOnMainThread (() => {
				sync.Set ();
			});
			sync.Wait ();
		}

		public IConfiguration Configuration {
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

				if (this.Dal == null) {
					XmlDocument document = LoadMetadata ();
					Uri uri = new Uri (Settings.Url);

					//to create db here
					DbContext.Current.InitDatabase (Settings.BaseUrl);
					IOfflineContext context = SyncContext.Current.CreateOfflineContext (document, uri.Host, uri);

					var configuration = document.DocumentElement;
					Settings.ConfigName = configuration.Attributes ["Name"].Value;
					Settings.ConfigVersion = configuration.Attributes ["Version"].Value;
					Settings.WriteSettings ();

					IDictionary<string, string> deviceInfo = GetDeviceInfo ();

					this.Dal = DalContext.Current.CreateDal
						(context
						, Settings.Application
						, Settings.Language
						, Settings.UserName
						, Settings.Password
						, Settings.ConfigName
						, Settings.ConfigVersion
						, deviceInfo
						, UpdateLoadStatus);

					this._commonData = ValueStackContext.Current.CreateCommonData ();

					this.Settings.ConfigName = Dal.ConfigName;
					this.Settings.ConfigVersion = Dal.ConfigVersion;
				}

				LogonComplete (clearCache);

			} catch (CustomException ex) {
				Logon (clearCache, ex.FriendlyMessage);
			} catch (UriFormatException) {
				Logon (clearCache, D.INVALID_ADDRESS);
			}
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

		void UpdateLoadStatus (int total, int processed)
		{
			if (total != 0 && _progressController != null) {
				_controller.BeginInvokeOnMainThread (() => {
					_progressController.UpdateStatus (total, processed);
				});
			}
		}

		IBusinessProcess LoadBusinessProcess ()
		{
			ValueStack = ValueStackContext.Current.CreateValueStack (_exceptionHandler);
			ValueStack.Push ("context", this);
			ValueStack.Push ("isTablet", UIDevice.CurrentDevice.Model.Contains ("iPad"));

			_configuration = BusinessProcessContext.Current.CreateConfigurationFactory ().CreateConfiguration (ValueStack);

			return BusinessProcessContext.Current.CreateBusinessProcessFactory ().CreateBusinessProcess (_configuration.BusinessProcess.File, ValueStack);
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
			Dal.UpdateCredentials (Settings.UserName, Settings.Password);
			Dal.LoadSolution (clearCache, this.Settings.SyncOnStart, LoadComplete);
		}

		void LoadComplete (object sender, ISyncEventArgs args)
		{
			if (args.Ok)
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
			this._commonData.UserId = Dal.UserId;
			this._businessProcess = LoadBusinessProcess ();

			bool success = this._businessProcess != null;
			if (!success) {
				if (!inSync)
					Dal.RefreshAsync (LoadComplete);
				else
					throw new Exception ("Couldn't load context");
			} else {

				if (Settings.DevelopModeEnabled) {
					BusinessProcessContext.Current.InitConsole (Settings.WaitDebuggerEnabled);
					BusinessProcessContext.Current.InitConsole (Settings.WaitDebuggerEnabled, DbContext.Current.Database);
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
