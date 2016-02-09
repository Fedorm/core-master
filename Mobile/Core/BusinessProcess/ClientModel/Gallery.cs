using BitMobile.Application;
using BitMobile.Script;
using BitMobile.Utilities.IO;
using System;

namespace BitMobile.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global, UnusedMember.Global, IntroduceOptionalParameters.Global, UnusedAutoPropertyAccessor.Global
    public class Gallery
    {
        private readonly IApplicationContext _context;
        private readonly ScriptEngine _scriptEngine;

        public Gallery(IApplicationContext context, ScriptEngine engine)
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

        public void Copy(string destinationPath, IJSExecutable handler)
        {
            Copy(destinationPath, handler, null);
        }

        public void Copy(string destinationPath, IJSExecutable callback, object state)
        {
            string path = FileSystemProvider.TranslatePath(_context.LocalStorage, destinationPath);

            Action<object, CallbackArgs> handler;
            if (callback != null)
                handler = (s, args) =>
                    callback.ExecuteStandalone(_scriptEngine.Visitor, s, args);
            else
                handler = (s, args) => { };

            _context.GalleryProvider.Copy(path, Size, handler, state);
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

