using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace BitMobile.Controls
{
    public class WebMapGoogle : WebView
    {
        GoogleMapBehavior _bahavior = new GoogleMapBehavior();

        object _loadingSync = new object();
        bool _isLoaded = false;

        public WebMapGoogle()
        {
            
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

		public override UIView CreateView ()
		{
			base.CreateView ();

			_view.LoadFinished += HandleLoadFinished;
			_view.ScalesPageToFit = false;

			return _view;
		}

        protected override void LoadPage()
        {
			_view.LoadHtmlString(_bahavior.Page, null);
        }

		void HandleLoadFinished(object sender, EventArgs e)
		{
			lock (_loadingSync)
			{
				_isLoaded = true;

				_view.EvaluateJavascript(_bahavior.BuildInitializeFunction());
			}
		}
    }
}

