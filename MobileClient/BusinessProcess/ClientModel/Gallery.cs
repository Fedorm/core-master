using System;
using BitMobile.Application.IO;
using BitMobile.Common.Application;
using BitMobile.Common.ScriptEngine;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    public class Gallery
    {
        private readonly IApplicationContext _context;
        private readonly IScriptEngine _scriptEngine;

        public Gallery(IApplicationContext context, IScriptEngine engine)
        {
            _context = context;
            _scriptEngine = engine;

            Size = 200;
        }

        public int Size { get; set; }

        public void Copy(string destinationPath)
        {
            Copy(destinationPath, null);
        }

        public void Copy(string destinationPath, IJsExecutable handler)
        {
            Copy(destinationPath, handler, null);
        }

        public void Copy(string destinationPath, IJsExecutable callback, object state)
        {
            string path = IOContext.Current.TranslateLocalPath( destinationPath);

            Action<bool> handler;
            if (callback != null)
                handler = result =>
                    callback.ExecuteCallback(_scriptEngine.Visitor, state, new CallbackArgs(result));
            else
                handler = result => { };

            _context.GalleryProvider.Copy(path, Size, handler);
        }

        public class CallbackArgs
        {
            public CallbackArgs(bool result)
            {
                Result = result;
            }

            public bool Result { get; private set; }
        }
    }
}

