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
    public class Camera
    {
        private readonly IApplicationContext _context;
        private readonly IScriptEngine _scriptEngine;

        public Camera(IScriptEngine scriptEngine, IApplicationContext context)
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

        public void MakeSnapshot(IJsExecutable callback)
        {
            MakeSnapshot(Path, Size, callback, null);
        }

        public void MakeSnapshot(IJsExecutable callback, object state)
        {
            MakeSnapshot(Path, Size, callback, state);
        }

        public void MakeSnapshot(string path)
        {
            MakeSnapshot(path, Size, null, null);
        }

        public void MakeSnapshot(string path, IJsExecutable callback)
        {
            MakeSnapshot(path, Size, callback, null);
        }

        public void MakeSnapshot(string path, IJsExecutable callback, object state)
        {
            MakeSnapshot(path, Size, callback, state);
        }

        public void MakeSnapshot(int size)
        {
            MakeSnapshot(Path, size, null, null);
        }

        public void MakeSnapshot(int size, IJsExecutable callback)
        {
            MakeSnapshot(Path, size, callback, null);
        }

        public void MakeSnapshot(int size, IJsExecutable callback, object state)
        {
            MakeSnapshot(Path, size, callback, state);
        }

        public void MakeSnapshot(string path, int size)
        {
            MakeSnapshot(path, size, null, null);
        }

        public void MakeSnapshot(string path, int size, IJsExecutable callback)
        {
            MakeSnapshot(path, size, callback, null);
        }

        public void MakeSnapshot(string path, int size, IJsExecutable callback, object state)
        {
            string p = IOContext.Current.TranslateLocalPath(path);

            Action<bool> handler;
            if (callback != null)
                handler = result =>
                    callback.ExecuteCallback(_scriptEngine.Visitor, state, new CallbackArgs(result));
            else
                handler = res => { };

            _context.CameraProvider.MakeSnapshot(p, size, handler);
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
