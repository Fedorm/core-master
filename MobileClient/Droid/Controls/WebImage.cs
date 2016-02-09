using System;
using BitMobile.Common.Controls;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "WebImage")]
    // ReSharper disable UnusedMember.Global
    class WebImage : WebView
    {
        Type _urlType;

        public WebImage(BaseScreen activity)
            : base(activity)
        {
        }

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
                        return BitBrowserApp.Current.Settings.BaseUrl + "/image/" + base.Url;
                    default:
                        return base.Url;
                }
            }
            set
            {
                base.Url = value;
            }
        }

        public override void CreateView()
        {
            base.CreateView();
            _view.Settings.SetLayoutAlgorithm(Android.Webkit.WebSettings.LayoutAlgorithm.SingleColumn);
        }

        enum Type
        {
            Absolute,
            Relative
        }
    }
}