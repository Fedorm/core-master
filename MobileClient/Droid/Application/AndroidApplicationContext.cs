using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using Android.Content;
using Android.Views;
using Android.Widget;
using BitMobile.Application;
using BitMobile.Application.BusinessProcess;
using BitMobile.Application.Controls;
using BitMobile.Application.DataAccessLayer;
using BitMobile.Application.DbEngine;
using BitMobile.Application.Exceptions;
using BitMobile.Application.IO;
using BitMobile.Application.Log;
using BitMobile.Application.Translator;
using BitMobile.Application.ValueStack;
using BitMobile.Bulder;
using BitMobile.Common.Application;
using BitMobile.Common.Application.Tracking;
using BitMobile.Common.BusinessProcess.SolutionConfiguration;
using BitMobile.Common.BusinessProcess.WorkingProcess;
using BitMobile.Common.Controls;
using BitMobile.Common.DataAccessLayer;
using BitMobile.Common.Device.Providers;
using BitMobile.Common.SyncLibrary;
using BitMobile.Common.Utils;
using BitMobile.Common.ValueStack;
using BitMobile.Droid.Controls;
using BitMobile.Droid.Providers;
using BitMobile.Droid.PushNotifications;
using BitMobile.Droid.Screens;
using BitMobile.Droid.StyleSheet;
using Java.Lang;
using Exception = System.Exception;
using IBusinessProcess = BitMobile.Common.BusinessProcess.WorkingProcess.IBusinessProcess;
using String = System.String;
using Thread = System.Threading.Thread;

namespace BitMobile.Droid.Application
{
    class AndroidApplicationContext : IApplicationContext, IDisposable
    {
        private const string FilesystemDirectory = "filesystem";

        private readonly ICommonData _commonData;
        private readonly BaseScreen _baseActivity;
        private readonly Settings _settings;
        private readonly Queue<Action> _executionQueue = new Queue<Action>();
        private IConfiguration _configuration;
        private IBusinessProcess _businessProcess;
        private Action _loadComplete;
        private ImageCache _currentImageCache;
        private bool _disposed;

        public AndroidApplicationContext(BaseScreen baseActivity, Settings settings, Action loadComplete)
        {
            ApplicationBackground += () => { };
            ApplicationRestore += () => { };

            GlobalVariables = new Dictionary<string, object>();

            _baseActivity = baseActivity;
            _settings = settings;
            _loadComplete = loadComplete;

            LocationProvider = new GpsProvider(_baseActivity);
            LocationTracker = new GpsTracker(_baseActivity);
            GalleryProvider = new GalleryProvider(_baseActivity, this);
            CameraProvider = new CameraProvider(_baseActivity, this);
            DialogProvider = new DialogProvider(_baseActivity, this);
            DisplayProvider = new DisplayProvider();
            ClipboardProvider = new ClipboardProvider(_baseActivity);
            EmailProvider = new EmailProvider(_settings, _baseActivity);
            JokeProviderInternal = new JokeProvider(_baseActivity);
            LocalNotificationProvider = new LocalNotificationProvider(_baseActivity);
            WebProvider = new WebProvider(_baseActivity);

            var builder = new SolutionBuilder(this);
            builder.Build();

            _commonData = ValueStackContext.Current.CreateCommonData("Android");
        }

        public event Progress LoadingProgress;
        public event Action<string, string> ReturnToStartMenu;

        public Screen CurrentNativeScreen
        {
            get
            {
                if (CurrentScreen == null)
                    return null;
                return (Screen)CurrentScreen.Screen;
            }
        }

        internal DialogProvider DialogProviderInternal
        {
            get { return (DialogProvider)DialogProvider; }
        }

        internal JokeProvider JokeProviderInternal { get; private set; }

        public void Start()
        {
            LoadApplication();

            if (_settings.WaitDebuggerEnabled)
                BusinessProcessContext.Current.InitConsole(DbContext.Current.Database);
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

        #region IApplicationContext

        public IConfiguration Configuration { get { return _configuration; } }

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

        public IApplicationSettings Settings
        {
            get { return _settings; }
        }

		public IGalleryProvider GalleryProvider { get; private set;}

		public ICameraProvider CameraProvider { get; private set;}

		public IDialogProvider DialogProvider { get; private set;}

		public IDisplayProvider DisplayProvider { get; private set;}

		public IClipboardProvider ClipboardProvider { get; private set;}

		public IEmailProvider EmailProvider { get; private set;}

		public ILocalNotificationProvider LocalNotificationProvider { get; private set;}

		public IWebProvider WebProvider { get; private set;}

        public bool InBackground { get; private set; }

        public event Action ApplicationBackground;

        public event Action ApplicationRestore;

        public bool OpenScreen(string screenName, String controllerName
            , Dictionary<string, object> parameters = null
            , bool isBackCommand = false
            , bool isRefresh = false)
        {
            try
            {
                ControlsContext.Current.ActionHandlerLocker.Acquire();

                LogManager.Logger.ScreenOpening(screenName, controllerName, parameters);

                _baseActivity.ClearNavigationEvents();

                DateTime startLoading = DateTime.Now;

                ValueStack = ValueStackContext.Current.CreateValueStack(BitBrowserApp.Current.ExceptionHandler);
                ValueStack.Push("common", _commonData);
                ValueStack.Push("context", this);
                ValueStack.Push("dao", Dal.Dao);
                ValueStack.Push("activity", _baseActivity);
                ValueStack.Push("isTablet", IsTablet());

                foreach (var variable in GlobalVariables)
                    ValueStack.Push(variable.Key, variable.Value);

                if (parameters != null)
                    foreach (KeyValuePair<String, object> item in parameters)
                        ValueStack.Push(item.Key, item.Value);

                var controller = BusinessProcessContext.Current.CreateScreenController(controllerName);
                ValueStack.SetCurrentController(controller);
                ValueStack.Push("controller", controller);
                screenName = controller.GetView(screenName);

                var imageCashe = new ImageCache(this);
                var scr = (Screen)BusinessProcessContext.Current.CreateScreenFactory()
                    .CreateScreen(screenName, ValueStack, controller, imageCashe);
                var currentScreen = ControlsContext.Current.CreateScreenData(screenName, controllerName, scr);

                FlipScreen(scr.View, isBackCommand, isRefresh);

                if (Settings.DevelopModeEnabled)
                {
                    Runtime runtime = Runtime.GetRuntime();
                    long usedMemInMb = (runtime.TotalMemory() - runtime.FreeMemory()) / 1048576L;
                    long maxHeapSizeInMb = runtime.MaxMemory() / 1048576L;
                    string memory = string.Format("{0}/{1}", usedMemInMb, maxHeapSizeInMb);

                    string message = string.Format("{0}\n{1}\n{2}"
                        , screenName
                        , (DateTime.Now - startLoading).ToString("ss\\.ff")
                        , memory);
                    Toast.MakeText(_baseActivity, message, ToastLength.Short).Show();
                }

                ThreadPool.QueueUserWorkItem(Finish, new object[] { CurrentScreen, _currentImageCache });

                CurrentScreen = currentScreen;
                _currentImageCache = imageCashe;

                return true;
            }
            catch (Exception e)
            {
                HandleException(e);

                return false;
            }
        }

        public void RefreshScreen(Dictionary<String, object> parameters = null)
        {
            OpenScreen(CurrentScreen.Name, CurrentScreen.ControllerName, parameters, false, true);

            _baseActivity.HideSoftInput();
        }

        public void InvokeOnMainThread(Action action)
        {
            _executionQueue.Enqueue(action);
            ThreadPool.QueueUserWorkItem(InvokeOnMainThreadCallback);
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
            BitBrowserApp.Current.ExceptionHandler.Handle(e);
        }

        public bool Validate(string arg)
        {
            arg = arg.Trim();
            bool result = arg.ToLower() == ValueStackConst.ValidateAll
                ? ((IValidatable)CurrentScreen.Screen).Validate()
                : arg.Split(';').Select(item => ValueStack.Peek(item.Trim())).OfType<IValidatable>().Aggregate(true, (current, validatable) => current & validatable.Validate());

            return result;
        }

        public void PhoneCall(string number)
        {
            var intent = new Intent(Intent.ActionCall);
            var uri = Android.Net.Uri.Parse(string.Format("tel:{0}", number));
            intent.SetData(uri);
            _baseActivity.StartActivity(intent);
        }

        public string LocalStorage
        {
            get
            {
                string solution = IOContext.Current.GetSolutionName(new UriBuilder(Settings.BaseUrl).Uri);

                return Path.Combine(BitBrowserApp.RootPath
                    , solution
                    , FilesystemDirectory
                    , Dal.UserId.ToString());
            }
        }

        public async void ScanBarcode(Action<object> callback)
        {
            var scanner = new ZXing.Mobile.MobileBarcodeScanner();
            var result = await scanner.Scan();
            if (!ReferenceEquals(result, null))
                _baseActivity.InvokeOnResume(() => callback(result.Text));
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
            {
                _settings.ClearCacheOnStart = true;
                BitBrowserApp.Current.SyncSettings(false);
            }
            Dispose();

            _baseActivity.ShutDown();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                if (Dal != null)
                    Dal.Dispose();

                _disposed = true;
            }
        }
        #endregion

        void LoadApplication()
        {
            try
            {
                if (Dal == null)
                {
                    IDictionary<string, string> deviceInfo = BitBrowserApp.Current.GetDeviceInfo();

                    XmlDocument document = LoadMetadata();
                    var uri = new UriBuilder(_settings.Url).Uri;

                    DbContext.Current.InitDatabase(_settings.BaseUrl);
                    IOfflineContext context = BitMobile.Application.SyncLibrary.SyncContext.Current.CreateOfflineContext(document, uri.Host, uri);

                    var configuration = document.DocumentElement;
                    Settings.ConfigName = configuration.Attributes["Name"].Value;
                    Settings.ConfigVersion = configuration.Attributes["Version"].Value;
                    Settings.WriteSettings();

                    Dal = DalContext.Current.CreateDal(context
                        , _settings.Application
                        , _settings.Language
                        , _settings.UserName
                        , _settings.Password
                        , Settings.ConfigName
                        , Settings.ConfigVersion
                        , deviceInfo
                        , (total, processed) =>
                        {
                            if (LoadingProgress != null)
                                LoadingProgress(total, processed, D.LOADING_SOLUTION);
                        });
                }

                Dal.UpdateCredentials(_settings.UserName, _settings.Password);

                Dal.LoadSolution(_settings.ClearCacheOnStart, _settings.SyncOnStart, LoadComplete);
            }
            catch (CustomException e)
            {
                ReturnToStartMenu(e.FriendlyMessage, e.Report);
            }
            catch (UriFormatException e)
            {
                ReturnToStartMenu(D.INVALID_ADDRESS, D.INVALID_ADDRESS);
            }
            catch (Exception e)
            {
                ReturnToStartMenu(D.LOADING_ERROR, e.ToString());
            }
        }

        private XmlDocument LoadMetadata()
        {
            try
            {
               
                LoadingProgress(-1, -1, D.INITIALISING);

                string path = string.Format(@"\metadata\{0}", _settings.Url.GetHashCode());

                using (IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    bool fileExist = isoFile.FileExists(path);
                    if (_settings.ClearCacheOnStart || !fileExist)
                    {
                        DalContext.Current.Ping(_settings.Url);

                        string uri = _settings.Url + "GetClientMetadata";

                        WebRequest req = WebRequest.Create(uri.ToCurrentScheme(ApplicationContext.Current.Settings.HttpsDisabled));
                        string svcCredentials =
                            Convert.ToBase64String(Encoding.ASCII.GetBytes(_settings.UserName + ":" + _settings.Password));
                        req.Headers.Add("Authorization", "Basic " + svcCredentials);

                        using (WebResponse resp = req.GetResponse())
                        using (Stream responseStream = resp.GetResponseStream())
                        {
                            if (fileExist)
                                isoFile.DeleteFile(path);

                            using (IsolatedStorageFileStream fileStream = isoFile.CreateFile(path))
                                responseStream.CopyTo(fileStream);
                        }
                    }

                    using (IsolatedStorageFileStream fileStream = isoFile.OpenFile(path, FileMode.Open))
                    {
                        var doc = new XmlDocument();
                        doc.Load(fileStream);
                        return doc;
                    }
                }
            }
            catch (XmlException e)
            {
                throw new NonFatalException(D.UNEXPECTED_ANSWER_OPEN_WEB_BROWSER, "XmlException has been thrown during the load metadata operation", e);
            }
            catch (WebException e)
            {
                throw new ConnectionException("WebException has been thrown during the load metadata operation", e);
            }
        }

        void LoadComplete(object sender, ISyncEventArgs e)
        {
            _baseActivity.RunOnUiThread(() =>
            {
                if (!_disposed)
                    if (e.Ok)
                        StartApplication();
                    else
                    {
                        var ce = e.Exception as ConnectionException;
                        if (ce != null)
                            ReturnToStartMenu(ce.FriendlyMessage, ce.Report);
                        else
                        {
                            HandleException(e.Exception);
                            var ex = e.Exception as CustomException;
                            if (ex != null)
                                ReturnToStartMenu(ex.FriendlyMessage, ex.Report);
                        }
                    }
            });
        }

        void StartApplication()
        {
            _commonData.UserId = Dal.UserId;

            ValueStack = ValueStackContext.Current.CreateValueStack(BitBrowserApp.Current.ExceptionHandler);
            ValueStack.Push("context", this);
            ValueStack.Push("common", _commonData);
            ValueStack.Push("isTablet", IsTablet());

            try
            {
                _configuration = BusinessProcessContext.Current.CreateConfigurationFactory().CreateConfiguration(ValueStack);
            }
            catch (ResourceNotFoundException e)
            {
                ReturnToStartMenu(e.FriendlyMessage, e.Report);
                return;
            }

            try
            {
                _businessProcess = BusinessProcessContext.Current.CreateBusinessProcessFactory()
                    .CreateBusinessProcess(_configuration.BusinessProcess.File, ValueStack);
            }
            catch (ResourceNotFoundException e)
            {
                ReturnToStartMenu(e.FriendlyMessage, e.Report);
                return;
            }

            InitializePushNotifications();

            LoadingProgress = null;

            _loadComplete();
            _loadComplete = null;

            _settings.SetClearCacheDisabled();

            _businessProcess.Start(this);
        }

        void InitializePushNotifications()
        {
            if (!Settings.PushDisabled)
            {
                Manager pnm = PushNotificationsManagerFactory.CreateInstance(this,
                    BusinessProcessContext.Current.GlobalEventsController,
                    _settings.BaseUrl,
                    _commonData.UserId.ToString(),
                    _settings.Password,
                    BitBrowserApp.Current.DeviceId);
                pnm.StartRegisterDevice(_baseActivity.BaseContext);
            }
        }

        bool IsTablet()
        {
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            bool result = (_baseActivity.Resources.Configuration.ScreenLayout & Android.Content.Res.ScreenLayout.SizeMask) >= Android.Content.Res.ScreenLayout.SizeLarge;

            return result;
        }

        void Finish(object state)
        {
            Thread.Sleep(600);

            InvokeOnMainThread(() =>
            {
                    LogManager.Logger.ScreenOpened();

                    var st = (object[])state;
                    var sd = st[0] as IScreenData;
                    if (sd != null)
                        sd.Dispose();

                    var cache = st[1] as ImageCache;
                    if (cache != null)
                        cache.Dispose();

                BitBrowserApp.Current.RecycleInitialScreens();
                FindFirstResponder(CurrentScreen.Screen);


                GC.Collect();
                    GC.Collect();
                    GC.Collect();

                ControlsContext.Current.ActionHandlerLocker.Release();
            });
        }

        void FlipScreen(View view, bool isBackCommand = false, bool isRefresh = false)
        {
            _baseActivity.FlipScreen(view, isBackCommand, isRefresh);
        }

        bool FindFirstResponder(object obj)
        {
            var control = (IControl<View>)obj;

            var focusable = control as IFocusable;
            if (focusable != null)
                if (focusable.AutoFocus && control.View != null && control.View.Focusable)
                {
                    control.View.RequestFocus();
                    return true;
                }

            var container = control as IContainer;
            if (container != null)
                return container.Controls.Any(FindFirstResponder);

            return false;
        }

        void InvokeOnMainThreadCallback(object state)
        {
            _baseActivity.RunOnUiThread(() =>
            {
                var action = _executionQueue.Dequeue();
                if (action != null)
                    action();
            });
        }
    }
}