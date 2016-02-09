using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Webkit;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.Droid.UI;

namespace BitMobile.Droid.Controls
{
    // ReSharper disable RedundantExtendsListEntry, MemberCanBeProtected.Global, UnusedMemberHiearchy.Global, VirtualMemberNeverOverriden.Global, UnusedParameter.Global
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "WebView")]
    public class WebView : Control<WebView.CustomWebView>, IStyledObject
    {
        // ReSharper disable once PublicConstructorInAbstractClass
        public WebView(BaseScreen activity)
            : base(activity)
        {
        }

        public virtual string Url { get; set; }

        public override void CreateView()
        {
            _view = new CustomWebView(Activity);
            _view.Settings.JavaScriptEnabled = true;
            _view.SetWebViewClient(new InternalWebViewClient(this));
            _view.LayoutInvoke += View_LayoutInvoke;
        }

        public virtual void OnPageStarted(string url, Android.Graphics.Bitmap favicon)
        {
        }

        public virtual void OnPageFinished(string url)
        {
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            // nope
            return styleBound;
        }

        protected virtual void LoadPage()
        {
            _view.LoadUrl(Url);
        }

        private void View_LayoutInvoke(bool changed, int l, int t, int r, int b)
        {
            LoadPage();
        }

        public class CustomWebView : Android.Webkit.WebView
        {
            public CustomWebView(Context activity)
                : base(activity)
            {
            }

            public event LayoutHandler LayoutInvoke;

            protected override void OnLayout(bool changed, int l, int t, int r, int b)
            {
                base.OnLayout(changed, l, t, r, b);
                if (LayoutInvoke != null)
                    LayoutInvoke(changed, l, t, r, b);
            }
        }

        class InternalWebViewClient : WebViewClient
        {
            readonly WebView _owner;

            public InternalWebViewClient(WebView owner)
            {
                _owner = owner;
            }

            public override bool ShouldOverrideUrlLoading(Android.Webkit.WebView view, string url)
            {
                view.LoadUrl(url);
                return true;
            }

            public override void OnPageFinished(Android.Webkit.WebView view, string url)
            {
                base.OnPageFinished(view, url);
                _owner.OnPageFinished(url);
            }

            public override void OnPageStarted(Android.Webkit.WebView view, string url, Android.Graphics.Bitmap favicon)
            {
                base.OnPageStarted(view, url, favicon);
                _owner.OnPageStarted(url, favicon);
            }
        }
    }
}