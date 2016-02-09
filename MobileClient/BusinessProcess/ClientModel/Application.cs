using System;
using System.IO;
using System.Net;
using BitMobile.Application.Archivation;
using BitMobile.Application.DbEngine;
using BitMobile.Application.Exceptions;
using BitMobile.Application.IO;
using BitMobile.Application.Log;
using BitMobile.Common;
using BitMobile.Common.Application;
using BitMobile.Common.IO;
using BitMobile.Common.Log;
using BitMobile.Common.ScriptEngine;
using BitMobile.BusinessProcess.Factory;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable UnusedMember.Global
    class Application
    {
        private readonly IScriptEngine _scriptEngine;
        private readonly IApplicationContext _context;

        public Application(IScriptEngine scriptEngine, IApplicationContext context)
        {
            _scriptEngine = scriptEngine;
            _context = context;
        }

        public string ResourceVersion
        {
            get { return _context.Dal.ResourceVersion; }
        }

        public string CoreVersion
        {
            get { return CoreInformation.CoreVersion.ToString(); }
        }

        public void Exit()
        {
            _context.Exit(false);
        }

        public void Logout()
        {
            _context.Exit(true);
        }

        public bool SendDatabase()
        {
            try
            {
                using (Stream stream = DbContext.Current.GetDatabaseStream())
                using (Stream zippedStream = GZip.ZipStream(stream))
                {
                    IRemoteProvider provider = IOContext.Current.CreateWebDavProvider(IOContext.Current.LogDirectory);
                    string fileName = string.Format("{0}.db.zip", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
                    provider.SaveFile(fileName, zippedStream);
                    return true;
                }
            }
            catch (WebException e)
            {
                _context.HandleException(new ConnectionException("Error during SendDatabase operation", e));
                return false;
            }

        }

        public void ClearLog()
        {
            LogManager.Logger.ClearLog();
        }

        // ReSharper disable IntroduceOptionalParameters.Global
        public void Feedback(string title, string text)
        {
            Feedback(title, text, null, null);
        }

        public void Feedback(string title, string text, IJsExecutable handler)
        {
            Feedback(title, text, handler, null);
        }

        public void ClearControllersCache()
        {
            ControllerFactory.CreateInstance().ClearCache();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public async void Feedback(string title, string text, IJsExecutable handler, object value)
        {
            bool result = await LogManager.Reporter.Send(title, text, ReportType.Feedback);
            if (handler != null)
                _context.InvokeOnMainThread(
                    () => handler.ExecuteCallback(_scriptEngine.Visitor, value, new Args<bool>(result)));
        }
    }
}
