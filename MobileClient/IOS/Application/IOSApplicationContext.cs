using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using BitMobile.Application;
using BitMobile.Application.BusinessProcess;
using BitMobile.Application.Controls;
using BitMobile.Application.DataAccessLayer;
using BitMobile.Application.DbEngine;
using BitMobile.Application.Exceptions;
using BitMobile.Application.IO;
using BitMobile.Application.Log;
using BitMobile.Application.StyleSheet;
using BitMobile.Application.SyncLibrary;
using BitMobile.Application.Translator;
using BitMobile.Application.ValueStack;
using BitMobile.Bulder;
using BitMobile.Common.Application;
using BitMobile.Common.Application.Tracking;
using BitMobile.Common.BusinessProcess.Controllers;
using BitMobile.Common.BusinessProcess.SolutionConfiguration;
using BitMobile.Common.BusinessProcess.WorkingProcess;
using BitMobile.Common.Controls;
using BitMobile.Common.DataAccessLayer;
using BitMobile.Common.Device.Providers;
using BitMobile.Common.SyncLibrary;
using BitMobile.Common.Utils;
using BitMobile.Common.ValueStack;
using BitMobile.Controls;
using BitMobile.IOS.Providers;
using MonoTouch.Foundation;
using MonoTouch.Security;
using MonoTouch.UIKit;
using ZXing;
using ZXing.Mobile;
using IBusinessProcess = BitMobile.Common.BusinessProcess.WorkingProcess.IBusinessProcess;

namespace BitMobile.IOS
{
    // ReSharper disable once InconsistentNaming
    public class IOSApplicationContext : IApplicationContext
    {
        private readonly NavigationController _controller;
        private readonly CustomExceptionHandler _exceptionHandler;
        private readonly Queue<Action> _executionQueue = new Queue<Action>();
        private Func<bool> _back;
        private int _backgroundTaskId;

        private IBusinessProcess _businessProcess;
        private ICommonData _commonData;
        private IConfiguration _configuration;
        private Func<bool> _forward;
        private LogonController _logonController;
        private ProgressController _progressController;

        public IOSApplicationContext(AppDelegate appDelegate, NavigationController controller,
            ApplicationSettings settings, CustomExceptionHandler exceptionHandler)
        {
            ApplicationBackground += () => { };
            ApplicationRestore += () => { };

            GlobalVariables = new Dictionary<string, object>();

            Settings = settings;
            _controller = controller;
            _exceptionHandler = exceptionHandler;

			LocationProvider = new GpsProvider ();
            LocationTracker = new GPSTracker();
            GalleryProvider = new GalleryProvider(controller, this);
            CameraProvider = new CameraProvider(controller, this);
            DialogProvider = new DialogProvider(this);
            DisplayProvider = new DisplayProvider();
            ClipboardProvider = new ClipboardProvider();
            EmailProvider = new EmailProvider(settings, appDelegate);
            JokeProviderInternal = new JokeProvider();
            LocalNotificationProvider = new LocalNotificationProvider();
            WebProvider = new WebProvider();

            var builder = new SolutionBuilder(this);
            builder.Build();

            StyleSheetContext.Current.Scale = UIScreen.MainScreen.Scale;
        }

        public static bool Busy { get; private set; }

        public static Version OSVersion
        {
            get
            {
                Version result;
                if (!Version.TryParse(UIDevice.CurrentDevice.SystemVersion, out result))
                    result = new Version();
                return result;
            }
        }

        public NavigationController MainController
        {
            get { return _controller; }
        }

        public Screen RootControl
        {
            get
            {
                if (CurrentScreen != null && CurrentScreen.Screen != null)
                    return (Screen)CurrentScreen.Screen;
                return null;
            }
        }

        public JokeProvider JokeProviderInternal { get; private set; }

        public static string UniqueID
        {
            get
            {
                var query = new SecRecord(SecKind.GenericPassword);
                query.Service = NSBundle.MainBundle.BundleIdentifier;
                query.Account = "BitMobile";

                NSData uniqueId = SecKeyChain.QueryAsData(query);
                if (uniqueId == null)
                {
                    query.ValueData = NSData.FromString(Guid.NewGuid().ToString());
                    SecStatusCode err = SecKeyChain.Add(query);
                    if (err != SecStatusCode.Success && err != SecStatusCode.DuplicateItem)
                        return "[Cannot_store_Unique_ID]";

                    return query.ValueData.ToString();
                }
                return uniqueId.ToString();
            }
        }

        public string LocalStorage
        {
            get
            {
                string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..",
                    "Library");

                string solution = IOContext.Current.GetSolutionName(new Uri(Settings.BaseUrl));

                return Path.Combine(root, solution, "filesystem", Dal.UserId.ToString());
            }
        }

        public ICommonData CommonData
        {
            get { return _commonData; }
        }
        
        public bool SubscribeEvent(string name, Func<bool> action)
        {
            switch (name)
            {
                case "Back":
                    _back += action;
                    return true;
                case "Forward":
                    _forward += action;
                    return true;
            }

            return false;
        }

        public UIView GetFirstResponder()
        {
            var screen = _controller.TopViewController as ScreenController;
            if (screen != null)
            {
                return screen.GetFirstResponder();
            }
            throw new Exception("TopViewController isn't ScreenController");
        }

        public IDictionary<string, string> GetDeviceInfo()
        {
            var result = new Dictionary<string, string>();

            result.Add("deviceId", UniqueID);

            return result;
        }

		public void Start (bool clearCache)
        {
            if (clearCache || string.IsNullOrWhiteSpace(Settings.UserName) || string.IsNullOrWhiteSpace(Settings.Password) || Settings.WaitDebuggerEnabled)
            {
                Logon(clearCache);
                return;
            }

            LoadApplication (clearCache);                
        }

        private void LoadApplication(bool clearCache)
        {
            if (string.IsNullOrWhiteSpace(Settings.UserName) || string.IsNullOrWhiteSpace(Settings.Password))
            {
                if (Settings.WaitDebuggerEnabled)
                    BusinessProcessContext.Current.InitConsole();

                Logon(clearCache);
                return;
            }

            try
            {
                _backgroundTaskId = UIApplication.SharedApplication.BeginBackgroundTask(() => { });

                if (Dal == null)
                {
                    XmlDocument document = LoadMetadata();
                    var uri = new Uri(Settings.Url);

                    //to create db here
                    DbContext.Current.InitDatabase(Settings.BaseUrl);
                    IOfflineContext context = SyncContext.Current.CreateOfflineContext(document, uri.Host, uri);

                    XmlElement configuration = document.DocumentElement;
                    Settings.ConfigName = configuration.Attributes["Name"].Value;
                    Settings.ConfigVersion = configuration.Attributes["Version"].Value;
                    Settings.WriteSettings();

                    IDictionary<string, string> deviceInfo = GetDeviceInfo();

                    Dal = DalContext.Current.CreateDal
                        (context
                            , Settings.Application
                            , Settings.Language
                            , Settings.UserName
                            , Settings.Password
                            , Settings.ConfigName
                            , Settings.ConfigVersion
                            , deviceInfo
                            , UpdateLoadStatus);

                    _commonData = ValueStackContext.Current.CreateCommonData("IOS");

                    Settings.ConfigName = Dal.ConfigName;
                    Settings.ConfigVersion = Dal.ConfigVersion;
                }

                LogonComplete(clearCache);
            }
            catch (CustomException ex)
            {
                Logon(clearCache, ex.FriendlyMessage);
            }
            catch (UriFormatException)
            {
                Logon(clearCache, D.INVALID_ADDRESS);
            }
        }

        private XmlDocument LoadMetadata()
        {
            try
            {
                DalContext.Current.Ping(Settings.Url);

                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    Settings.Url.GetHashCode().ToString());

                bool fileExists = File.Exists(path);

                if (Settings.ClearCacheOnStart || !fileExists)
                {
                    string uri = Settings.Url + "GetClientMetadata";

                    WebRequest request = WebRequest.Create(uri.ToCurrentScheme(ApplicationContext.Current.Settings.HttpsDisabled));
                    string credentials =
                        Convert.ToBase64String(Encoding.ASCII.GetBytes(Settings.UserName + ":" + Settings.Password));
                    request.Headers.Add("Authorization", "Basic " + credentials);

                    using (WebResponse resp = request.GetResponse())
                    using (Stream responseStream = resp.GetResponseStream())
                    {
                        if (fileExists)
                            File.Delete(path);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                            responseStream.CopyTo(fileStream);
                    }
                }

                using (var stream = new FileStream(path, FileMode.Open))
                {
                    var doc = new XmlDocument();
                    doc.Load(stream);
                    return doc;
                }
            }
            catch (XmlException ex)
            {
                throw new NonFatalException(D.UNEXPECTED_ANSWER_OPEN_WEB_BROWSER,
                    "XmlException has been thrown during the load metadata operation", ex);
            }
            catch (WebException ex)
            {
                throw new ConnectionException("WebException has been thrown during the load metadata operation", ex);
            }
        }

        private void UpdateLoadStatus(int total, int processed)
        {
            if (total != 0 && _progressController != null)
            {
                _controller.BeginInvokeOnMainThread(() => { _progressController.UpdateStatus(total, processed); });
            }
        }

        private IBusinessProcess LoadBusinessProcess()
        {
            ValueStack = ValueStackContext.Current.CreateValueStack(_exceptionHandler);
            ValueStack.Push("context", this);
            ValueStack.Push("common", CommonData);
            ValueStack.Push("isTablet", UIDevice.CurrentDevice.Model.Contains("iPad"));

            _configuration = BusinessProcessContext.Current.CreateConfigurationFactory().CreateConfiguration(ValueStack);

            return
                BusinessProcessContext.Current.CreateBusinessProcessFactory()
                    .CreateBusinessProcess(_configuration.BusinessProcess.File, ValueStack);
        }

        private void Logon(bool clearCache, String message = null)
        {
            if (_logonController == null)
                _logonController = new LogonController(clearCache, Settings, LoadApplication, message);
            else
                _logonController.UpdateMessage(message);
            _controller.SetViewControllers(new UIViewController[] { _logonController }, false);
        }

        private void LogonComplete(bool clearCache)
        {
            Sync(clearCache);
            CreateStatusView();
        }

        private void CreateStatusView()
        {
            if (_progressController == null)
                _progressController = new ProgressController(Settings);
            _controller.SetViewControllers(new UIViewController[] { _progressController }, false);
        }

        private void Sync(bool clearCache)
        {
            Dal.UpdateCredentials(Settings.UserName, Settings.Password);
            Dal.LoadSolution(clearCache, Settings.SyncOnStart, LoadComplete);
        }

        private void LoadComplete(object sender, ISyncEventArgs args)
        {
            _controller.BeginInvokeOnMainThread(() =>
            {
                if (args.Ok)
                {
                    OpenStartScreen(true);
                    UIApplication.SharedApplication.EndBackgroundTask(_backgroundTaskId);
                }
                else
                {
                    var ce = args.Exception as CustomException;
                    if (ce != null)
                    {
                        string msg = ce.FriendlyMessage;
                        Logon(Settings.ClearCacheOnStart, msg);
                    }
                    else
                        HandleException(args.Exception);
                    UIApplication.SharedApplication.EndBackgroundTask(_backgroundTaskId);
                }
            });
        }

        private void OpenStartScreen(bool inSync)
        {
            CommonData.UserId = Dal.UserId;
            _businessProcess = LoadBusinessProcess();

            bool success = _businessProcess != null;
            if (!success)
            {
                if (!inSync)
                    Dal.RefreshAsync(LoadComplete);
                else
                    throw new Exception("Couldn't load context");
            }
            else
            {
                if (Settings.WaitDebuggerEnabled)
                {
                    BusinessProcessContext.Current.InitConsole();
                    BusinessProcessContext.Current.InitConsole(DbContext.Current.Database);
                }

                _businessProcess.Start(this);
            }

            if (_logonController != null)
            {
                _logonController.Dispose();
                _logonController = null;
            }
            if (_progressController != null)
            {
                _progressController.Dispose();
                _progressController = null;
            }

            AppDelegate.NotificationManager.SendToken();
        }

        private void InvokeOnMainThreadCallback()
        {
            Action action = _executionQueue.Dequeue();

            action();
        }

        public void OnApplicationBackground()
        {
            InBackground = true;
            ApplicationBackground();
        }

        public void OnApplicationForeground()
        {
            InBackground = false;
            ApplicationRestore();
        }

        #region IApplicationContext implementation

        public IConfiguration Configuration
        {
            get { return _configuration; }
        }

        public IValueStack ValueStack { get; private set; }

        public IDictionary<string, object> GlobalVariables { get; private set; }

        public IWorkflow Workflow
        {
            get { return _businessProcess != null ? _businessProcess.Workflow : null; }
        }

        public IDal Dal { get; private set; }

        public IScreenData CurrentScreen { get; private set; }

        public ILocationProvider LocationProvider { get; private set; }

        public ITracker LocationTracker { get; private set; }

        public IApplicationSettings Settings { get; private set; }

        public IGalleryProvider GalleryProvider { get; private set; }

        public ICameraProvider CameraProvider { get; private set; }

        public IDialogProvider DialogProvider { get; private set; }

        public IDisplayProvider DisplayProvider { get; private set; }

        public IClipboardProvider ClipboardProvider { get; private set; }

        public IEmailProvider EmailProvider { get; private set; }

        public ILocalNotificationProvider LocalNotificationProvider { get; private set; }

		public IWebProvider WebProvider { get; private set;}

        public bool InBackground { get; private set; }

        public event Action ApplicationBackground;

        public event Action ApplicationRestore;

        public bool OpenScreen(String screenName
            , string controllerName, Dictionary<String
                , object> parameters = null
            , bool isBackCommand = false
            , bool isRefresh = false)
        {
            IDisposable rootControl = null;
            try
            {
                Busy = true;

                LogManager.Logger.ScreenOpening(screenName, controllerName, parameters);

                _back = null;
                _forward = null;

                ValueStack = ValueStackContext.Current.CreateValueStack(_exceptionHandler);
                ValueStack.Push("common", CommonData);
                ValueStack.Push("context", this);
                ValueStack.Push("dao", Dal.Dao);
                ValueStack.Push("isTablet", UIDevice.CurrentDevice.Model.Contains("iPad"));

                foreach (var variable in GlobalVariables)
                    ValueStack.Push(variable.Key, variable.Value);

                if (parameters != null)
                {
                    foreach (var item in parameters)
                    {
                        ValueStack.Push(item.Key, item.Value);
                    }
                }

                IScreenController newController = BusinessProcessContext.Current.CreateScreenController(controllerName);
                ValueStack.SetCurrentController(newController);
                ValueStack.Push("controller", newController);
                screenName = newController.GetView(screenName);

                TabOrderManager.Create(this);

                var scr =
                    (Screen)
                        BusinessProcessContext.Current.CreateScreenFactory()
                            .CreateScreen(screenName, ValueStack, newController, null);

                rootControl = RootControl;
                CurrentScreen = ControlsContext.Current.CreateScreenData(screenName, controllerName, scr);

                var viewController = new ScreenController(scr.View);

                if (!isRefresh)
                {
                    _controller.SetViewControllers(new[]
                    {
                        _controller.TopViewController
                    }, false);

                    if (!isBackCommand)
                    {
                        _controller.PushViewController(viewController, true);
                    }
                    else
                    {
                        _controller.SetViewControllers(new[]
                        {
                            viewController,
                            _controller.TopViewController
                        }, false);
                        _controller.PopViewControllerAnimated(true);
                    }
                }
                else
                {
                    _controller.PopViewControllerAnimated(false);
                    _controller.PushViewController(viewController, false);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                LogManager.Logger.ScreenOpened();

                if (rootControl != null)                
                    rootControl.Dispose();

                GC.Collect();

                ControlsContext.Current.ActionHandlerLocker.Release();
                Busy = false;
            }
            return true;
        }

        public void RefreshScreen(Dictionary<String, object> parameters)
        {
            OpenScreen(CurrentScreen.Name, CurrentScreen.ControllerName, parameters, false, true);
        }

        public void InvokeOnMainThread(Action action)
        {
            _executionQueue.Enqueue(action);

            var nsa = new NSAction(InvokeOnMainThreadCallback);

            _controller.BeginInvokeOnMainThread(nsa);
        }

        public void InvokeOnMainThreadSync(Action action)
        {
            var sync = new ManualResetEventSlim();
            InvokeOnMainThread(() =>
            {
                action();
                sync.Set();
            });
            sync.Wait();
        }

        public void HandleException(Exception e)
        {
            _exceptionHandler.Handle(e);
        }

        public bool Validate(string args)
        {
            bool result;

            args = args.Trim();
            if (args.ToLower() == ValueStackConst.ValidateAll)
            {
                result = RootControl.Validate();
            }
            else
            {
                result = true;
                foreach (string item in args.Split(';'))
                {
                    var validatable = ValueStack.Peek(item.Trim()) as IValidatable;
                    if (validatable != null)
                        result &= validatable.Validate();
                }
            }

            return result;
        }

        public async void ScanBarcode(Action<object> callback)
        {
            var scanner = new MobileBarcodeScanner();
            Result result = await scanner.Scan();

			if (!ReferenceEquals(result, null))
                callback(result.Text);
        }

        public void PhoneCall(string number)
        {
            try
            {
                var url = new NSUrl(string.Format("tel://{0}", number));
                UIApplication.SharedApplication.OpenUrl(url);
            }
            catch
            {
            }
        }

        public void Wait()
        {
            Dal.Wait();

            var sync = new ManualResetEventSlim();
            InvokeOnMainThread(sync.Set);
            sync.Wait();
        }

        public void Exit(bool clearCache)
        {
            if (clearCache)
                NSUserDefaults.StandardUserDefaults.SetBool(true, IOS.Settings.KeyClearCacheOnStart);

            AppDelegate.ShutDownApplication();
        }

        #endregion
    }
}