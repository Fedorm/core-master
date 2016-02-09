using System.Linq;
using System.Xml;
using Android.Content;
using Android.Views;
using Android.Widget;
using BitMobile.Application;
using BitMobile.BusinessProcess;
using BitMobile.Common;
using BitMobile.Common.Entites;
using BitMobile.Controls;
using BitMobile.DataAccessLayer;
using BitMobile.DbEngine;
using BitMobile.Droid.Providers;
using BitMobile.Droid.Screens;
using BitMobile.Factory;
using BitMobile.SyncLibrary.BitMobile;
using BitMobile.Utilities.Exceptions;
using BitMobile.Utilities.IO;
using BitMobile.Utilities.Translator;
using BitMobile.ValueStack;
using Microsoft.Synchronization.ClientServices;
using Microsoft.Synchronization.ClientServices.IsolatedStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using Entity = BitMobile.SyncLibrary.BitMobile.Entity;

namespace BitMobile.Droid
{
    class ApplicationContext : IApplicationContext, IDisposable
    {
        const string FilesystemDirectory = "filesystem";

        readonly CommonData _commonData = new CommonData();
        readonly BaseScreen _baseActivity;
        readonly Settings _settings;
        readonly Queue<System.Action> _executionQueue = new Queue<System.Action>();
        Configuration.Configuration _configuration;
        BusinessProcess.BusinessProcess _businessProcess;
        System.Action _loadComplete;
        bool _disposed;

        public ApplicationContext(BaseScreen baseActivity, Settings settings, System.Action loadComplete)
        {
            GlobalVariables = new Dictionary<string, object>();

            _baseActivity = baseActivity;
            _settings = settings;
            _loadComplete = loadComplete;

            LocationProvider = new GpsProvider(baseActivity);

            LocationTracker = new GPSTracker(baseActivity);

            GalleryProvider = new GalleryProvider(baseActivity);

            CameraProvider = new CameraProvider(baseActivity, this);

            DialogProvider = new DialogProvider(baseActivity, this);

            ClipboardProvider = new ClipboardProvider(baseActivity, this);

            Application.ApplicationContext.InitContext(this);
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

        public void Start()
        {
            if (_settings.DevelopModeEnabled)
                InitConsole();

            LoadApplication();

            if (_settings.DevelopModeEnabled)
                InitConsole(DbEngine.Database.Current);
        }

        #region IApplicationContext

        public Configuration.Configuration Configuration { get { return _configuration; } }

        public ValueStack.ValueStack ValueStack { get; private set; }

        public IDictionary<string, object> GlobalVariables { get; private set; }

        public Workflow Workflow
        {
            get { return _businessProcess != null ? _businessProcess.Workflow : null; }
        }

        public DAL DAL { get; private set; }

        public ScreenData CurrentScreen { get; private set; }

        public ILocationProvider LocationProvider { get; private set; }

        public Tracker LocationTracker { get; private set; }

        public ApplicationSettings Settings
        {
            get { return _settings; }
        }

        public IGalleryProvider GalleryProvider { get; private set; }

        public ICameraProvider CameraProvider { get; private set; }

        public IDialogProvider DialogProvider { get; private set; }

        public IClipboardProvider ClipboardProvider { get; private set; }

        public bool OpenScreen(string screenName, String controllerName
            , Dictionary<string, object> parameters = null
            , bool isBackCommand = false
            , bool isRefresh = false)
        {
            try
            {
                ActionHandler.Busy = true;
                ActionHandlerEx.Busy = true;

                _baseActivity.ClearNavigationEvents();

                DateTime startLoading = DateTime.Now;

                ValueStack = new ValueStack.ValueStack(BitBrowserApp.Current.ExceptionHandler);
                ValueStack.Push("common", _commonData);
                ValueStack.Push("context", this);
                ValueStack.Push("dao", DAL.DAO);
                ValueStack.Push("activity", _baseActivity);

                foreach (var variable in GlobalVariables)
                    ValueStack.Push(variable.Key, variable.Value);

                if (parameters != null)
                    foreach (KeyValuePair<String, object> item in parameters)
                        ValueStack.Push(item.Key, item.Value);

                var controller = ControllerFactory.CreateInstance().CreateController<Controllers.ScreenController>(controllerName);
                ValueStack.Push("controller", controller);
                screenName = controller.GetView(screenName);

                var scr = (Screen)ScreenFactory.CreateInstance().CreateScreen<AndroidStyleSheet>(screenName, ValueStack, controller);
                var currentScreen = new ScreenData(screenName, controllerName, scr);

                FlipScreen(scr.View, isBackCommand, isRefresh);

                if (Settings.DevelopModeEnabled)
                {
                    string message = string.Format("{0}\n{1}"
                        , screenName
                        , (DateTime.Now - startLoading).ToString("ss\\.ff"));
                    Toast.MakeText(_baseActivity, message, ToastLength.Short).Show();
                }

                ThreadPool.QueueUserWorkItem(Finish, CurrentScreen);

                CurrentScreen = currentScreen;

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

        public void InvokeOnMainThread(System.Action action)
        {
            _executionQueue.Enqueue(action);
            ThreadPool.QueueUserWorkItem(InvokeOnMainThreadCallback);
        }

        public void InvokeOnMainThreadSync(System.Action action)
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
            bool result = arg.ToLower() == BitMobile.ValueStack.ValueStack.VALIDATE_ALL
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
                string solution = FileSystemProvider.GetSolutionName(new Uri(Settings.BaseUrl));

                return Path.Combine(BitBrowserApp.RootPath
                    , solution
                    , FilesystemDirectory
                    , DAL.UserId.ToString());
            }
        }

        public async void ScanBarcode(Action<object> callback)
        {
            var scanner = new ZXing.Mobile.MobileBarcodeScanner(_baseActivity);
            var result = await scanner.Scan();
            if (result != null)
                _baseActivity.InvokeOnResume(() => callback(result.Text));
        }

        public void Wait()
        {
            DAL.Wait();

            var sync = new ManualResetEventSlim();
            InvokeOnMainThread(sync.Set);
            sync.Wait();
        }

        public void Exit(bool clearCache)
        {
            if (clearCache)
            {
                Settings.ClearCacheOnStart = true;
                BitBrowserApp.Current.SyncSettings(false);
            }
            Dispose();

            _baseActivity.Finish();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                if (DAL != null)
                    DAL.Dispose();

                _disposed = true;
            }
        }
        #endregion

        void InitConsole(DbEngine.Database db = null)
        {
            if (ControllerFactory.Debugger == null)
                ControllerFactory.Debugger = Debugger.Debugger.CreateInstance(_settings.WaitDebuggerEnabled);
            else
                // ReSharper disable once PossibleNullReferenceException
                (ControllerFactory.Debugger as DbEngine.IDatabaseAware).SetDatabase(db);
        }

        void LoadApplication()
        {
            try
            {
                if (DAL == null)
                {
                    EntityFactory.CreateInstance = Entity.CreateInstance;
                    EntityFactory.DbRefFactory = DbRef.CreateInstance;
                    EntityFactory.CustomDictionaryFactory = () => new CustomDictionary();

                    IDictionary<string, string> deviceInfo = BitBrowserApp.Current.GetDeviceInfo();
                    
                    XmlDocument document = LoadMetadata();
                    var uri = new Uri(_settings.Url);
                    
                    Database.Init(_settings.BaseUrl);
                    IsolatedStorageOfflineContext context = new OfflineContext(document, uri.Host, uri);

                    var configuration = document.DocumentElement;
					Settings.ConfigName = configuration.Attributes ["Name"].Value;
					Settings.ConfigVersion = configuration.Attributes ["Version"].Value;
					Settings.WriteSettings();

                    DAL = new DAL(context
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
                        }, CacheRequestFactory);
                }

                DAL.UpdateCredentials(_settings.UserName, _settings.Password);

                DAL.LoadSolution(_settings.ClearCacheOnStart, _settings.SyncOnStart, LoadComplete);
            }
            catch (CustomException e)
            {
                ReturnToStartMenu(e.FriendlyMessage, e.Report);
            }
            catch (UriFormatException)
            {
                ReturnToStartMenu(D.INVALID_ADDRESS, D.INVALID_ADDRESS);
            }
            catch (Exception e)
            {
                ReturnToStartMenu(D.LOADING_ERROR, e.ToString());
            }
        }

        CacheRequestHandler CacheRequestFactory(Uri serviceUri, CacheControllerBehavior behaviors, AsyncWorkerManager manager)
        {
            return new HttpCacheRequestHandler(serviceUri, behaviors, manager);
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
                        string uri = _settings.Url + "GetClientMetadata";

                        WebRequest req = WebRequest.Create(uri);
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
            catch (WebException e)
            {
                throw new ConnectionException("WebException has been thrown during the load metadata operation", e);
            }
        }

        void LoadComplete(object sender, SyncEventArgs e)
        {
            _baseActivity.RunOnUiThread(() =>
            {
                if (!_disposed)
                    if (e.OK)
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
            _commonData.UserId = DAL.UserId;

            ValueStack = new ValueStack.ValueStack(BitBrowserApp.Current.ExceptionHandler);
            ValueStack.Push("context", this);
            ValueStack.Push("isTablet", IsTablet());

            try
            {
                _configuration = ConfigurationFactory.CreateInstance().CreateConfiguration(ValueStack);
            }
            catch (ResourceNotFoundException e)
            {
                ReturnToStartMenu(e.FriendlyMessage, e.Report);
                return;
            }

            try
            {
                _businessProcess = BusinessProcessFactory.CreateInstance().CreateBusinessProcess(_configuration.BusinessProcess.File, ValueStack);
            }
            catch (ResourceNotFoundException e)
            {
                ReturnToStartMenu(e.FriendlyMessage, e.Report);
                return;
            }

            LoadingProgress = null;

            _loadComplete();
            _loadComplete = null;

            _settings.SetClearCacheDisabled();

            _businessProcess.Start(this);
        }

        bool IsTablet()
        {
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            bool result = (_baseActivity.Resources.Configuration.ScreenLayout & Android.Content.Res.ScreenLayout.SizeMask) >= Android.Content.Res.ScreenLayout.SizeLarge;

            return result;
        }

        void Finish(object state)
        {
            Thread.Sleep(500);

            InvokeOnMainThread(() =>
                {
                    var sd = state as ScreenData;
                    if (sd != null)
                        ((IDisposable)sd.Screen).Dispose();

                    BitBrowserApp.Current.RecycleInitialScreens();

                    FindFirstResponder(CurrentScreen.Screen);
                    
                    GC.Collect();

                    ActionHandler.Busy = false;
                    ActionHandlerEx.Busy = false;
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
                System.Action action = _executionQueue.Dequeue();
                action();
            });
        }
    }
}