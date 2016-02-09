using System;

namespace BitMobile.Controls
{
	public class WebImage : WebView
	{
		Type _urlType;

		public WebImage ()
		{

		}

		public string UrlType {
			get { return _urlType.ToString (); }
			set {                
				if (!Enum.TryParse<Type> (value, true, out _urlType))
					throw new Exception ("Uknown type of Url: " + value);
			}
		}

		public override string Url {
			get {
				switch (_urlType) {
				case Type.Absolute:
					return base.Url;
				case Type.Relative:
					return BitMobile.Application.ApplicationContext.Context.Settings.BaseUrl + "/image/" + base.Url;
				default:
					return base.Url;
				}
			}
			set {
				base.Url = value;
			}
		}

		public override MonoTouch.UIKit.UIView CreateView ()
		{
			base.CreateView ();

			_view.ScalesPageToFit = true;

			return _view;
		}

		enum Type
		{
			Absolute,
			Relative
		}
	}
}

