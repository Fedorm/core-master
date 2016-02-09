using BitMobile.Application;
using BitMobile.Script;
using BitMobile.Utilities.IO;
using System;

namespace BitMobile.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global, UnusedMember.Global, IntroduceOptionalParameters.Global, ClassNeverInstantiated.Global, UnusedAutoPropertyAccessor.Global
    public class Camera
    {
        private readonly IApplicationContext _context;
        private readonly ScriptEngine _scriptEngine;

        public Camera(ScriptEngine scriptEngine, IApplicationContext context)
        {
            _scriptEngine = scriptEngine;
            _context = context;

            Size = 200;
        }

        public string Path { get; set; }

        public int Size { get; set; }

        public void MakeSnapshot()
        {
            MakeSnapshot(Path, Size, null, null);
        }

        public void MakeSnapshot(IJSExecutable callback)
        {
            MakeSnapshot(Path, Size, callback, null);
        }

        public void MakeSnapshot(IJSExecutable callback, object state)
        {
            MakeSnapshot(Path, Size, callback, state);
        }

        public void MakeSnapshot(string path)
        {
            MakeSnapshot(path, Size, null, null);
        }

        public void MakeSnapshot(string path, IJSExecutable callback)
        {
            MakeSnapshot(path, Size, callback, null);
        }

        public void MakeSnapshot(string path, IJSExecutable callback, object state)
        {
            MakeSnapshot(path, Size, callback, state);
        }

        public void MakeSnapshot(int size)
        {
            MakeSnapshot(Path, size, null, null);
        }

        public void MakeSnapshot(int size, IJSExecutable callback)
        {
            MakeSnapshot(Path, size, callback, null);
        }

        public void MakeSnapshot(int size, IJSExecutable callback, object state)
        {
            MakeSnapshot(Path, size, callback, state);
        }

        public void MakeSnapshot(string path, int size)
        {
            MakeSnapshot(path, size, null, null);
        }

        public void MakeSnapshot(string path, int size, IJSExecutable callback)
        {
            MakeSnapshot(path, size, callback, null);
        }

        public void MakeSnapshot(string path, int size, IJSExecutable callback, object state)
        {
            string p = FileSystemProvider.TranslatePath(_context.LocalStorage, path);

            Action<object, CallbackArgs> handler;
            if (callback != null)
                handler = (s, args) =>
                    callback.ExecuteStandalone(_scriptEngine.Visitor, new[] { s, args });
            else
                handler = (s, args) => { };

            _context.CameraProvider.MakeSnapshot(p, size, handler, state);
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
