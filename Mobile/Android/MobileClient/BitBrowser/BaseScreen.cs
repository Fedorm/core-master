using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using System;
using BitMobile.Utilities.Translator;
using Android.Hardware;
using BitMobile.Controls;
using System.Collections.Generic;
using Android.Graphics;
using BitMobile.Droid.Backgrounding;
using BitMobile.Factory;
using Orientation = Android.Widget.Orientation;

namespace BitMobile.Droid
{
    [Activity(MainLauncher = true, Icon = "@drawable/icon")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BaseScreen : Activity, ISensorEventListener
    {
        public const int CameraRequestCode = 1;
        public const int ReportEmailRequestCode = 2;
        public const int GalleryRequestCode = 3;

        CustomFlipper _flipper;

        bool _loaded;
        SensorManager _sensorManger;
        ServiceConnection _serviceConnection;

        bool _isInFront;
        Action _onResumeCallback;

        readonly Dictionary<int, Action<Result, Intent>> _activityResultCallbacks
            = new Dictionary<int, Action<Result, Intent>>();

        public int Height { get; private set; }
        public int Width { get; private set; }

        public event Func<bool> GoBack;

        public BaseScreen()
        {
        }

        public BaseService BackgroundService
        {
            get
            {
                if (_serviceConnection.Binder == null)
                    throw new Exception("Service not bound yet");
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

        public void StartActivityForResult(Intent intent, int requestCode, Action<Result, Intent> callback)
        {
            _activityResultCallbacks[requestCode] = callback;
            StartActivityForResult(intent, requestCode);
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
            Resources res = Resources;
            int reqWidth = Width;
            int reqHeight = Height;

            var options = new BitmapFactory.Options { InJustDecodeBounds = true };
            Bitmap image = null;
            try
            {
                image = BitmapFactory.DecodeResource(res, resId);
                options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);
                options.InJustDecodeBounds = false;

                return BitmapFactory.DecodeResource(res, resId, options);
            }
            finally
            {
                if (image != null)
                    image.Recycle();
            }
        }

        public void HideSoftInput()
        {
            if (CurrentFocus != null)
            {
                var inputMethodManager = (InputMethodManager)GetSystemService(InputMethodService);
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
                if (GoBack != null)
                    return GoBack();
                else
                    return false;

            return base.OnKeyDown(keyCode, e);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            menu.Clear();

            if (BitBrowserApp.Current.Settings != null)
                menu.Add(Menu.First, 1, 1, D.PREFERENCES);

            menu.Add(Menu.First, 2, 2, D.EXIT);
#if DEBUG
            menu.Add(Menu.First, 3, 3, "Throw exception");
#endif
            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
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

            return base.OnOptionsItemSelected(item);
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
                            ControllerFactory.GlobalEvents.OnApplicationShake(workflow);

                            //BitBrowserApp.Current.AppContext.ValueStack.TryCallScript("Events", "OnApplicationShake", workflow);
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
                throw new Exception("The application is already runing!");

            bool isPortrait = WindowManager.DefaultDisplay.Height > WindowManager.DefaultDisplay.Width;

            #region Set default android settings

            RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetSoftInputMode(SoftInput.AdjustPan);
            #endregion

            if (isPortrait)
            {
                _loaded = true;

                _sensorManger = (SensorManager)GetSystemService(SensorService);

                var intent = new Intent(this, typeof(BaseService));
                StartService(intent);
                _serviceConnection = new ServiceConnection(null);
                BindService(intent, _serviceConnection, Bind.AutoCreate);

                BitBrowserApp.Current.PrepareApplication(this);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            Action<Result, Intent> action;
            if (_activityResultCallbacks.TryGetValue(requestCode, out action))
            {
                action(resultCode, data);
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
                string workflow = BitBrowserApp.Current.AppContext.Workflow.Name;
                var eventsController = ControllerFactory.GlobalEvents;
                if (eventsController != null)
                    eventsController.OnApplicationRestore(workflow);
            }
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
                string workflow = BitBrowserApp.Current.AppContext.Workflow.Name;

                ControllerFactory.GlobalEvents.OnApplicationBackground(workflow);
            }

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

            Finish();
        }

        static int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            // Raw height and width of image
            int height = options.OutHeight;
            int width = options.OutWidth;
            int inSampleSize = 1;

            if (height > reqHeight || width > reqWidth)
            {
                int halfHeight = height / 2;
                int halfWidth = width / 2;

                // Calculate the largest inSampleSize value that is a power of 2 and keeps both
                // height and width larger than the requested height and width.
                while ((halfHeight / inSampleSize) > reqHeight
                        && (halfWidth / inSampleSize) > reqWidth)
                    inSampleSize *= 2;
            }

            return inSampleSize;
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

                _activity.Width = MeasureSpec.GetSize(widthMeasureSpec);
                _activity.Height = MeasureSpec.GetSize(heightMeasureSpec);
            }
        }
    }
}

