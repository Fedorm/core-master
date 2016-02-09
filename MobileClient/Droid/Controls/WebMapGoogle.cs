using System.Threading;
using Android.Views;
using BitMobile.Application.Controls;
using BitMobile.Common.Controls;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "WebMapGoogle")]
    // ReSharper disable UnusedMember.Global
    class WebMapGoogle : WebView
    {
        private readonly IWebMapBehavior _behavior;

        readonly object _loadingSync = new object();
        bool _isLoaded;

        public WebMapGoogle(BaseScreen activity)
            : base(activity)
        {
            _behavior = ControlsContext.Current.CreateWebMapBehavior();
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

        public override void CreateView()
        {
            base.CreateView();
            _view.ScrollBarStyle = ScrollbarStyles.OutsideOverlay;
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
                        BitBrowserApp.Current.AppContext.InvokeOnMainThread(() => _view.LoadUrl(script));
                    }
                });
        }

        protected override void LoadPage()
        {
            _view.LoadDataWithBaseURL("file:///android_asset/", _behavior.Page, "text/html", "UTF-8", "");
        }
    }
}