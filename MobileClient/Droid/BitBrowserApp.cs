using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using Android.Content;
using Android.Preferences;
using BitMobile.Application.BusinessProcess;
using BitMobile.Application.Log;
using BitMobile.Application.TestsAgent;
using BitMobile.Application.Translator;
using BitMobile.Common.Application.Exceptions;
using BitMobile.Common.Log;
using BitMobile.Droid.Application;
using BitMobile.Droid.Screens;
using BitMobile.Droid.Tests;

namespace BitMobile.Droid
{
    class BitBrowserApp
    {
        public static BitBrowserApp Current { get; private set; }

        private const string ErrorFile = @"\Error.log";

        public static  string RootPath;

        public static  string Temp;

        private BaseScreen _baseActivity;
        LogonScreen _logonScreen;
        ProgressScreen _progressScreen;
        JavaExceptionHandler _javaExceptionsHandler;
        // ReSharper disable once NotAccessedField.Local
        TestAgent _analyzer;

        static BitBrowserApp()
        {
            Current = new BitBrowserApp();
        }

        public AndroidApplicationContext AppContext { get; private set; }

        public IExceptionHandler ExceptionHandler { get; private set; }

        public Settings Settings { get; private set; }

        public bool Crashing { get; private set; }

        public Android.Content.Res.Resources Resources
        {
            get
            {
                return BaseActivity.Resources;
            }
        }

        public int Width { get { return BaseActivity.Width; } }

        public int Height { get { return BaseActivity.Height; } }

        public string DeviceId { get; private set; }

        public BaseScreen BaseActivity
        {
            get { return _baseActivity; }
        }

        public void PrepareApplication(BaseScreen baseActivity)
        {
            _baseActivity = baseActivity;

            RootPath = baseActivity.ApplicationContext.GetExternalFilesDir(null).AbsolutePath;
            Temp = Path.Combine(RootPath, "Temp");

            if (!Directory.Exists(RootPath))
                Directory.CreateDirectory(RootPath);

            string nomediaPath = Path.Combine(RootPath, ".nomedia");
            if (!File.Exists(nomediaPath))
                File.Create(nomediaPath);


            DeviceId = Android.Provider.Settings.Secure.GetString(Android.App.Application.Context.ContentResolver
                , Android.Provider.Settings.Secure.AndroidId);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            _javaExceptionsHandler = new JavaExceptionHandler();
            Java.Lang.Thread.DefaultUncaughtExceptionHandler = _javaExceptionsHandler;

            D.Init(BaseActivity.Resources.Configuration.Locale.Language);

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(BaseActivity.BaseContext);
            var infobases = new InfobasesScreen(BaseActivity, prefs, StartApplication);
            infobases.Start();
        }

        public void RecycleInitialScreens()
        {
            if (_logonScreen != null)
                _logonScreen.Recycle();
            _logonScreen = null;

            if (_progressScreen != null)
                _progressScreen.Recycle();
            _progressScreen = null;
        }

        void StartApplication(Settings settings)
        {
            Settings = settings;

            Settings.ReadSettings();
            Settings.WriteSettings();

            _logonScreen = new LogonScreen(BaseActivity, Settings, LoadApplication);
            _progressScreen = new ProgressScreen(BaseActivity, Settings);

            AppContext = new AndroidApplicationContext(BaseActivity, Settings, LoadComplete);
            AppContext.LoadingProgress += _progressScreen.Progress;
            AppContext.ReturnToStartMenu += OpenStartScreen;

            ExceptionHandler = new ExceptionHandler(Settings, BaseActivity, AppContext);
            HandleLastError();
        }

        public void OpenPreferences()
        {
            BaseActivity.StartActivity(typeof(PreferencesScreen));
        }

        public void SubscribeEvent(string name, Func<bool> action)
        {
            switch (name)
            {
                case "Back":
                    BaseActivity.GoBack += action;
                    return;
            }
        }

        public void SyncSettings(bool sharedPreferencePrefer = true)
        {
            if (sharedPreferencePrefer)
                Settings.ReadSettings();
            else
                Settings.WriteSettings();
        }

        public IDictionary<string, string> GetDeviceInfo()
        {
            var result = new Dictionary<string, string> { { "deviceId", DeviceId } };

            return result;
        }

        private void HandleLastError()
        {
            using (IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                bool fileExist = isoFile.FileExists(ErrorFile);

                if (fileExist)
                {
                    using (IsolatedStorageFileStream fileStream = isoFile.OpenFile(ErrorFile, FileMode.Open))
                    {
                        if (fileStream.Length > 0)
                        {
                            IReport report = LogManager.Reporter.CreateReport(fileStream);
                            ExceptionHandler.Handle(report, () => Prepare(true));
                        }
                        else
                            Prepare(false);
                    }
                }
                else
                    Prepare(false);
            }
        }

        void Prepare(bool loadingError)
        {
            using (IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication())
                if (isoFile.FileExists(ErrorFile))
                    isoFile.DeleteFile(ErrorFile);

            if (Settings.WaitDebuggerEnabled)
                BusinessProcessContext.Current.InitConsole();

            if (loadingError || Settings.IsInvalid || Settings.ClearCacheOnStart || Settings.WaitDebuggerEnabled)
                OpenStartScreen(D.TO_GET_STARTED_YOU_HAVE_TO_LOGIN);
            else
                LoadApplication();
        }

        void OpenStartScreen(string message, string exceptionMessage = null)
        {
            _logonScreen.Start(message, exceptionMessage);
            _progressScreen.Recycle();
        }

        void LoadApplication()
        {
            _progressScreen.Start();
            _logonScreen.Recycle();
            System.Threading.ThreadPool.QueueUserWorkItem(state =>
                {
                    System.Threading.Thread.Sleep(500);
                    BaseActivity.RunOnUiThread(() => AppContext.Start());
                });
        }

        void LoadComplete()
        {
            AppContext.LoadingProgress -= _progressScreen.Progress;

            if (Settings.TestAgentEnabled)
                _analyzer = new TestAgent(new AndroidViewProxy(AppContext, BaseActivity));
        }

        private static void WriteException(string e)
        {
            try
            {
                Current.Crashing = true;

                using (IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    bool fileExist = isoFile.FileExists(ErrorFile);

                    if (fileExist)
                        isoFile.DeleteFile(ErrorFile);

                    using (IsolatedStorageFileStream fileStream = isoFile.CreateFile(ErrorFile))
                        Current.ExceptionHandler.GetReport(true, e).Serialize(fileStream);
                }
            }
            finally
            {
                if (LogManager.Logger != null)
                    LogManager.Logger.Error(e, true);
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            WriteException(e != null ? e.ExceptionObject.ToString() : ".NET unhandled exception is null");
        }

        class JavaExceptionHandler : Java.Lang.Object, Java.Lang.Thread.IUncaughtExceptionHandler
        {
            public void UncaughtException(Java.Lang.Thread thread, Java.Lang.Throwable ex)
            {
                WriteException(ex != null ? ex.ToString() : "Java uncaught exception is null");
            }
        }
    }
}