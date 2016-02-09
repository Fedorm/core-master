using System;
using BitMobile.Application.Controls;
using BitMobile.Common.Controls;
using MonoTouch.UIKit;

namespace BitMobile.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "WebMapGoogle")]
    public class WebMapGoogle : WebView
    {
        private readonly IWebMapBehavior _bahavior;

        private readonly object _loadingSync = new object();
        private bool _isLoaded;

        public WebMapGoogle()
        {
            _bahavior = ControlsContext.Current.CreateWebMapBehavior();
        }

        public void AddMarker(string caption, double latitude, double longitude, string color)
        {
            lock (_loadingSync)
            {
                if (_isLoaded)
                    _view.EvaluateJavascript(_bahavior.BuildShowMarkerFunction(caption, latitude, longitude, color));
                else
                    _bahavior.AddMarker(caption, latitude, longitude, color);
            }
        }

        public override void CreateView()
        {
            base.CreateView();

            _view.LoadFinished += HandleLoadFinished;
            _view.ScalesPageToFit = false;
        }

        protected override void LoadPage()
        {
            if (_view != null)
                _view.LoadHtmlString(_bahavior.Page, null);
        }

        private void HandleLoadFinished(object sender, EventArgs e)
        {
            lock (_loadingSync)
            {
                {
                    _isLoaded = true;

                    if (_view != null)
                        _view.EvaluateJavascript(_bahavior.BuildInitializeFunction());
                }
            }
        }
    }
}