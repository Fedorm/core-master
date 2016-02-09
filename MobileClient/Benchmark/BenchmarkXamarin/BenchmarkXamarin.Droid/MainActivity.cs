using System;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace BenchmarkXamarin.Core.Droid
{
    [Activity(Label = "BenchmarkXamarin", MainLauncher = true, Icon = "@drawable/icon")]
    // ReSharper disable once UnusedMember.Global
    public class MainActivity : Activity
    {
        private BenchmarkManager _benchmarkManager;
        private TextView _console;
        private ScrollView _scroll;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);
            _console = FindViewById<TextView>(Resource.Id.console);
            _scroll = FindViewById<ScrollView>(Resource.Id.consoleScroll);

            _benchmarkManager = new BenchmarkManager();
            _benchmarkManager.Log += BenchmarkManagerOnLog;

            using (var buttonRun = FindViewById<Button>(Resource.Id.run))
            buttonRun.Click += ButtonRunOnClick;
        }
        
        private void BenchmarkManagerOnLog(string text)
        {
            _console.Text += "\n-> " + text;
            _scroll.FullScroll(FocusSearchDirection.Down);
        }
        
        private void ButtonRunOnClick(object sender, EventArgs eventArgs)
        {
            _benchmarkManager.Start();
        }
    }
}


