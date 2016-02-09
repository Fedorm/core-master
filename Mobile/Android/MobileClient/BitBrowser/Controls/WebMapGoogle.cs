using System.Threading;
using Android.Views;
using BitMobile.Droid;

namespace BitMobile.Controls
{
    // ReSharper disable UnusedMember.Global
    class WebMapGoogle : WebView
    {
        readonly GoogleMapBehavior _behavior = new GoogleMapBehavior();

        readonly object _loadingSync = new object();
        bool _isLoaded;

        public WebMapGoogle(BaseScreen activity)
            : base(activity)
        {
        }

        public void AddMarker(string caption, double latitude, double longitude, string color)
        {
            lock (_loadingSync)
            {
                if (_isLoaded)
                    _view.LoadUrl(string.Format("javascript:{0}"
                        , _behavior.BuildShowMarkerFunction(caption, latitude, longitude, color)));
                else
                    _behavior.AddMarker(caption, latitude, longitude, color);
            }
        }

        public override View CreateView()
        {
            base.CreateView();
            _view.ScrollBarStyle = ScrollbarStyles.OutsideOverlay;
            return _view;
        }

        public override void OnPageFinished(string url)
        {
            base.OnPageFinished(url);

            ThreadPool.QueueUserWorkItem(state =>
                {
                    Thread.Sleep(200);

                    lock (_loadingSync)
                    {
                        _isLoaded = true;
                        string script = string.Format("javascript:{0}", _behavior.BuildInitializeFunction());
                        _view.LoadUrl(script);
                    }
                });
        }

        protected override void LoadPage()
        {
            _view.LoadDataWithBaseURL("file:///android_asset/", _behavior.Page, "text/html", "UTF-8", "");
        }
    }
}