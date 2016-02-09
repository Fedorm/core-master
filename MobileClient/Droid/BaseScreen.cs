using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using System;
using BitMobile.Application.BusinessProcess;
using BitMobile.Application.Log;
using BitMobile.Application.Translator;
using Android.Hardware;
using System.Collections.Generic;
using Android.Graphics;
using BitMobile.Droid.Backgrounding;

namespace BitMobile.Droid
{
    [Activity(MainLauncher = true, Icon = "@drawable/icon"
        , ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.KeyboardHidden)]       
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BaseScreen : Activity, ISensorEventListener
    {
        private CustomFlipper _flipper;

        private static bool _loaded;
        private SensorManager _sensorManger;
        private ServiceConnection _serviceConnection;
        private bool _longPressed;
        private bool _isInFront;
        private Action _onResumeCallback;
        private bool _waitingActivityResult;

        readonly Dictionary<int, TaskCompletionSource<ActivityResult>> _activityResultCallbacks
            = new Dictionary<int, TaskCompletionSource<ActivityResult>>();

        public int Height { get; private set; }
        public int Width { get; private set; }

        public event Func<bool> GoBack;

        public BaseService BackgroundService
        {
            get
            {
                if (_serviceConnection.Binder == null)
                    return null;
                return _serviceConnection.Binder.Service;
            }
        }

        public void FlipScreen(int resource, bool isBackCommand = false, bool isRefresh = false)
        {
            if (_flipper == null)
            {
                _flipper = new CustomFlipper(this);
                SetContentView(_flipper);
            }

            var inflator = (LayoutInflater)GetSystemService(LayoutInflaterService);
            View view = inflator.Inflate(resource, null);

            FlipScreen(view, isBackCommand, isRefresh);
        }

        public Task<ActivityResult> StartActivityForResultAsync(Intent intent)
        {
            var tcs = new TaskCompletionSource<ActivityResult>();
            int requestCode = new Random().Next();
            _activityResultCallbacks[requestCode] = tcs;
            _waitingActivityResult = true;
            StartActivityForResult(intent, requestCode);
            return tcs.Task;
        }

        public void InvokeOnResume(Action action)
        {
            if (_isInFront)
                action();
            else
                _onResumeCallback = action;
        }

        public void ClearNavigationEvents()
        {
            GoBack = null;
        }

        public void FlipScreen(View view, bool isBackCommand = false, bool isRefresh = false)
        {
            HideSoftInput();

            View lastView = _flipper.CurrentView;

            if (lastView != null)
                UnregisterForContextMenu(lastView);
            RegisterForContextMenu(view);

            _flipper.AddView(view);

            if (isRefresh)
            {
                _flipper.SetInAnimation(this, Resource.Animation.refresh_in);
                _flipper.SetOutAnimation(this, Resource.Animation.refresh_out);
            }
            else if (!isBackCommand)
            {
                _flipper.SetInAnimation(this, Resource.Animation.go_next_in);
                _flipper.SetOutAnimation(this, Resource.Animation.go_next_out);
            }
            else
            {
                _flipper.SetInAnimation(this, Resource.Animation.go_prev_in);
                _flipper.SetOutAnimation(this, Resource.Animation.go_prev_out);
            }

            _flipper.ShowNext();

            _flipper.RemoveView(lastView);
        }

        public Bitmap DecodeBitmap(int resId)
        {
            return Helper.LoadBitmap(resId, Width, Height, Resources);
        }

        public void HideSoftInput()
        {
            if (CurrentFocus != null)
            {
                CurrentFocus.ClearFocus();

                using (var inputMethodManager = (InputMethodManager)GetSystemService(InputMethodService))
                    inputMethodManager.HideSoftInputFromWindow(CurrentFocus.WindowToken, 0);
            }
        }

        public void ShowSoftInput(View view)
        {
            var inputMethodManager = (InputMethodManager)GetSystemService(InputMethodService);
            inputMethodManager.ShowSoftInput(view, ShowFlags.Forced);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                e.StartTracking();
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                if (_longPressed)
                    OpenContextMenu(_flipper.CurrentView);
                else if (GoBack != null)
                    return GoBack();
                _longPressed = false;
                return false;
            }

            return base.OnKeyUp(keyCode, e);
        }

        public override bool OnKeyLongPress(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                _longPressed = true;
                return true;
            }

            return base.OnKeyLongPress(keyCode, e);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            CreateMenu(menu);
            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            MenuItemSelected(item);
            return base.OnOptionsItemSelected(item);
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            base.OnCreateContextMenu(menu, v, menuInfo);
            CreateMenu(menu);
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            MenuItemSelected(item);
            return base.OnContextItemSelected(item);
        }

        public void ShutDown()
        {
            if (LogManager.Logger != null)
                LogManager.Logger.ApplicationClosed();
            Finish();
        }

        #region ISensorEventListener

        DateTime _lastCheck;

        float _lastX;
        float _lastY;
        float _lastZ;

        float _baseX;
        float _baseY;
        float _baseZ;

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        { }

        public void OnSensorChanged(SensorEvent e)
        {
            if ((DateTime.Now - _lastCheck).TotalMilliseconds > 100)
            {
                float x = e.Values[0];
                float y = e.Values[1];
                float z = e.Values[2];

                if (Math.Abs(_lastX) > 0.000001)
                {
                    double lastLength = Math.Sqrt(Math.Abs(
                        Math.Pow((x - _lastX), 2) + Math.Pow((y - _lastY), 2) + Math.Pow((z - _lastZ), 2)
                        ));

                    double baseLength = Math.Sqrt(Math.Abs(
                        Math.Pow((x - _baseX), 2) + Math.Pow((y - _baseY), 2) + Math.Pow((z - _baseZ), 2)
                        ));

                    if (lastLength > 10 && baseLength < lastLength / 2)
                        if (BitBrowserApp.Current.AppContext != null
                            && BitBrowserApp.Current.AppContext.ValueStack != null)
                        {
                            string workflow = BitBrowserApp.Current.AppContext.Workflow.Name;
                            BusinessProcessContext.Current.GlobalEventsController.OnApplicationShake(workflow);
                        }
                }

                _lastCheck = DateTime.Now;

                _baseX = _lastX;
                _baseY = _lastY;
                _baseZ = _lastZ;

                _lastX = x;
                _lastY = y;
                _lastZ = z;
            }
        }
        #endregion

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            if (_loaded)
                Process.KillProcess(Process.MyPid());

            bool isPortrait = WindowManager.DefaultDisplay.Height > WindowManager.DefaultDisplay.Width;

            #region Set default android settings

            RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetSoftInputMode(SoftInput.AdjustPan);
            Window.RequestFeature(WindowFeatures.ActionBar);
            #endregion

            if (isPortrait)
            {
                _sensorManger = (SensorManager)GetSystemService(SensorService);

                var intent = new Intent(this, typeof(BaseService));
                StartService(intent);
                _serviceConnection = new ServiceConnection(null);
                BindService(intent, _serviceConnection, Bind.AutoCreate);

                BitBrowserApp.Current.PrepareApplication(this);

                _loaded = true;
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            TaskCompletionSource<ActivityResult> tcs;
            if (_activityResultCallbacks.TryGetValue(requestCode, out tcs))
            {                
                tcs.SetResult(new ActivityResult(resultCode, data));
                _activityResultCallbacks.Remove(requestCode);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            _isInFront = true;
            if (_onResumeCallback != null)
                _onResumeCallback();
            _onResumeCallback = null;

            if (_sensorManger != null)
                _sensorManger.RegisterListener(this, _sensorManger.GetDefaultSensor(SensorType.Accelerometer), SensorDelay.Ui);

            BitBrowserApp app = BitBrowserApp.Current;
            if (app.AppContext != null && app.AppContext.ValueStack != null && app.AppContext.Workflow != null)
            {
                BitBrowserApp.Current.AppContext.OnApplicationForeground();

                //todo: move to application context
                if (!_waitingActivityResult)
                {
                    string workflow = BitBrowserApp.Current.AppContext.Workflow.Name;
                    var eventsController = BusinessProcessContext.Current.GlobalEventsController;
                    if (eventsController != null)
                        eventsController.OnApplicationRestore(workflow);
                }                
            }
            
            if (LogManager.Reporter != null)
                LogManager.Logger.ApplicationMaximized();

            _waitingActivityResult = false; // OnResume execute after OnActivityResult
        }

        protected override void OnPause()
        {
            base.OnPause();
            _isInFront = false;

            if (_sensorManger != null)
                _sensorManger.UnregisterListener(this);

            if (BitBrowserApp.Current.AppContext != null
                && BitBrowserApp.Current.AppContext.ValueStack != null
                && BitBrowserApp.Current.AppContext.Workflow != null)
            {
                BitBrowserApp.Current.AppContext.OnApplicationBackground();

                //todo: move to application context
                if (!_waitingActivityResult)
                {
                    string workflow = BitBrowserApp.Current.AppContext.Workflow.Name;
                    BusinessProcessContext.Current.GlobalEventsController.OnApplicationBackground(workflow);
                }
            }

            if (LogManager.Reporter != null)
                LogManager.Logger.ApplicationMinimized();

            GC.Collect();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_loaded)
                Process.KillProcess(Process.MyPid());
        }

        void CloseApplication(object state, object result)
        {
            if (_loaded && BitBrowserApp.Current.Settings != null)
                BitBrowserApp.Current.Settings.LogOut();

            if (BitBrowserApp.Current.AppContext != null)
                BitBrowserApp.Current.AppContext.Dispose();

            ShutDown();
        }

        private static void CreateMenu(IMenu menu)
        {
            menu.Clear();

            if (BitBrowserApp.Current.Settings != null)
                menu.Add(Menu.First, 1, 1, D.PREFERENCES);

            menu.Add(Menu.First, 2, 2, D.EXIT);
#if DEBUG
            menu.Add(Menu.First, 3, 3, "Throw exception");
#endif
        }
        
        private void MenuItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case 1:
                    BitBrowserApp.Current.OpenPreferences();
                    break;
                case 2:
                    using (var builder = new AlertDialog.Builder(this))
                    {
                        builder.SetTitle(D.WARNING);
                        builder.SetMessage(D.CLOSING_QUESTION);
                        builder.SetPositiveButton(D.YES, CloseApplication);
                        builder.SetNegativeButton(D.NO, (sender, e) => { });
                        builder.Show();
                    }
                    break;
                case 3:
                    throw new Exception("Fake exception", new Exception("Fake inner exception"));
            }
        }

        private void UpdateSize(int width, int height)
        {
            // ugly hack
            if (width < height)
            {
                Width = width;
                Height = height;
            }
        }

        class CustomFlipper : ViewFlipper
        {
            readonly BaseScreen _activity;

            public CustomFlipper(BaseScreen activity)
                : base(activity)
            {
                _activity = activity;
            }

            protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
            {
                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

                _activity.UpdateSize(MeasureSpec.GetSize(widthMeasureSpec), MeasureSpec.GetSize(heightMeasureSpec));
            }
        }

        public struct ActivityResult
        {
            public ActivityResult(Result result, Intent data)
                : this()
            {
                Result = result;
                Data = data;
            }

            public Result Result { get; private set; }
            public Intent Data { get; private set; }
        }
    }
}

