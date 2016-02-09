using System.Linq;
using Android.Content;
using Android.Preferences;
using BitMobile.Droid;
using BitMobile.Droid.Screens;
using BitMobile.Droid.Tests;
using BitMobile.TestsAgent;
using BitMobile.Utilities.Exceptions;
using BitMobile.Utilities.LogManager;
using BitMobile.Utilities.Translator;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml;
using System.Xml.Serialization;

namespace BitMobile
{
    class BitBrowserApp
    {
        public static BitBrowserApp Current { get; private set; }

        public const string DaoFile = @"\lib\DAO_{0}.dll";
        public const string ErrorFile = @"\Error.log";

        public static readonly string RootPath;

        public static readonly string Temp;

        BaseScreen _baseActivity;
        LogonScreen _logonScreen;
        ProgressScreen _progressScreen;
        JavaExceptionHandler _javaExceptionsHandler;
        // ReSharper disable once NotAccessedField.Local
        TestAgent _analyzer;

        static BitBrowserApp()
        {
            string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            RootPath = Path.Combine(path, "FirstBit", "BitMobile");
            Temp = Path.Combine(RootPath, "Temp");

            Current = new BitBrowserApp();
        }

        public ApplicationContext AppContext { get; private set; }

        public CustomExceptionHandler ExceptionHandler { get; private set; }

        public Settings Settings { get; private set; }

        public Android.Content.Res.Resources Resources
        {
            get
            {
                return _baseActivity.Resources;
            }
        }

        public int Width { get { return _baseActivity.Width; } }

        public int Height { get { return _baseActivity.Height; } }

        public string DeviceId { get; private set; }

        public void PrepareApplication(BaseScreen baseActivity)
        {
            _baseActivity = baseActivity;

            DeviceId = Android.Provider.Settings.Secure.GetString(Android.App.Application.Context.ContentResolver
                , Android.Provider.Settings.Secure.AndroidId);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            _javaExceptionsHandler = new JavaExceptionHandler();
            Java.Lang.Thread.DefaultUncaughtExceptionHandler = _javaExceptionsHandler;

            D.Init(_baseActivity.Resources.Configuration.Locale.Language);

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(_baseActivity.BaseContext);
            var infobases = new InfobasesScreen(_baseActivity, prefs, StartApplication);
            infobases.Start();
        }

        void StartApplication(Settings settings)
        {
            Settings = settings;

            Settings.ReadSettings();
            Settings.WriteSettings();

            _logonScreen = new LogonScreen(_baseActivity, Settings, LoadApplication);
            _progressScreen = new ProgressScreen(_baseActivity, Settings);

            AppContext = new ApplicationContext(_baseActivity, Settings, LoadComplete);
            AppContext.LoadingProgress += _progressScreen.Progress;
            AppContext.ReturnToStartMenu += OpenStartScreen;

            ExceptionHandler = new ExceptionHandler(Settings, _baseActivity, AppContext);
            HandleLastError(Prepare);
        }

        public void OpenPreferences()
        {
            _baseActivity.StartActivity(typeof(PreferencesScreen));
        }

        public void RecycleInitialScreens()
        {
            if(_logonScreen != null)
                _logonScreen.Recycle();
            _logonScreen = null;

            if (_progressScreen != null)
                _progressScreen.Recycle();
            _progressScreen = null;
        }

        public bool SubscribeEvent(string name, Func<bool> action)
        {
            switch (name)
            {
                case "Back":
                    _baseActivity.GoBack += action;
                    return true;
            }

            return false;
        }

        public bool InvokeActions(String[] actions)
        {
            if (AppContext.Workflow != null)
                foreach (var a in AppContext.Workflow.RegisteredActions)
                {
                    if (actions.Any(s => a.ContainsWorkflowAction(s)))
                    {
                        a.InvokeAll(AppContext);
                        return true;
                    }
                }

            return false;
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

        private void HandleLastError(Action nextStep)
        {
            using (IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                bool fileExist = isoFile.FileExists(ErrorFile);

                if (fileExist)
                {
                    using (IsolatedStorageFileStream fileStream = isoFile.OpenFile(ErrorFile, FileMode.Open))
                    {
                        object report;
                        var serializer = new XmlSerializer(typeof(Log));
                        try
                        {
                            report = serializer.Deserialize(fileStream);
                        }
                        catch (XmlException)
                        {
                            fileStream.Position = 0;
                            using (var reader = new StreamReader(fileStream))
                            {
                                report = reader.ReadToEnd();
                            }
                        }

                        ExceptionHandler.Handle(report, nextStep);
                    }
                }
                else
                    nextStep();
            }
        }

        void Prepare()
        {
            using (IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication())
                if (isoFile.FileExists(ErrorFile))
                    isoFile.DeleteFile(ErrorFile);

            if (Settings.IsInvalid || (Settings.ClearCacheOnStart && !Settings.AnonymousAccess))
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
            System.Threading.ThreadPool.QueueUserWorkItem(state =>
                {
                    System.Threading.Thread.Sleep(500);
                    _baseActivity.RunOnUiThread(() => AppContext.Start());
                });
            _logonScreen.Recycle();
        }

        void LoadComplete()
        {
            AppContext.LoadingProgress -= _progressScreen.Progress;

            if (Settings.TestAgentEnabled)
                _analyzer = new TestAgent(new AndroidViewProxy(AppContext, _baseActivity));
        }

        public static void WriteException(string e)
        {
            using (IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                bool fileExist = isoFile.FileExists(ErrorFile);

                if (fileExist)
                    isoFile.DeleteFile(ErrorFile);

                using (IsolatedStorageFileStream fileStream = isoFile.CreateFile(ErrorFile))
                {
                    try
                    {
                        Log log = Current.ExceptionHandler.GetLog(true, e);
                        var serializer = new XmlSerializer(typeof(Log));
                        serializer.Serialize(fileStream, log);
                    }
                    catch (Exception ex)
                    {
                        string report = "Logger error: " + ex;
                        report += Environment.NewLine;
                        report += e;

                        using (var writer = new StreamWriter(fileStream))
                            writer.Write(report);
                    }
                }
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