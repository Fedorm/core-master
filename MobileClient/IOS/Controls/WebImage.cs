using System;
using BitMobile.Application;
using BitMobile.Common.Controls;
using MonoTouch.UIKit;

namespace BitMobile.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "WebImage")]
    public class WebImage : WebView
    {
        private Type _urlType;

        public string UrlType
        {
            get { return _urlType.ToString(); }
            set
            {
                if (!Enum.TryParse(value, true, out _urlType))
                    throw new Exception("Uknown type of Url: " + value);
            }
        }

        public override string Url
        {
            get
            {
                switch (_urlType)
                {
                    case Type.Absolute:
                        return base.Url;
                    case Type.Relative:
                        return ApplicationContext.Current.Settings.BaseUrl + "/image/" + base.Url;
                    default:
                        return base.Url;
                }
            }
            set { base.Url = value; }
        }

        public override void CreateView()
        {
            base.CreateView();

            _view.ScalesPageToFit = true;
        }

        private enum Type
        {
            Absolute,
            Relative
        }
    }
}