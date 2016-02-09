using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using BitMobile.Common.ScriptEngine;
using BitMobile.Application.Log;
using BitMobile.Common.Device.Providers;
using BitMobile.Application.Translator;
using BitMobile.Application;
using BitMobile.Common.Develop;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    public class Web
    {
        private IScriptEngine _scriptEngine;
        private IWebProvider _provider;

        public Web(IScriptEngine engine, IWebProvider provider)
        {
            Assert.IsNotNull(engine); Assert.IsNotNull(provider);

            _scriptEngine = engine;
            _provider = provider;
        }

        public WebRequest Request()
        {
            return Request(null);
        }

        public WebRequest Request(string host)
        {
            return new WebRequest(_scriptEngine) { Host = host };
        }

        public void OpenUrl(string url)
        {
            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                try
                {
                    _provider.OpenUrl(uri);
                }
                catch (Exception e)
                {
                    ApplicationContext.Current.HandleException(e);
                    throw _scriptEngine.CreateException(new Error("Exception", D.UNEXPECTED_ERROR_OCCURED));
                }
            else
            {
                throw _scriptEngine.CreateException(new Error("UriException", D.INVALID_ADDRESS));
            }
        }
    }
}