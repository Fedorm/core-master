using System;
using System.Collections.Generic;
using BitMobile.BusinessProcess.ClientModel;
using BitMobile.Common.Application;
using BitMobile.Common.Application.Tracking;
using BitMobile.Common.BusinessProcess.SolutionConfiguration;
using BitMobile.Common.BusinessProcess.WorkingProcess;
using BitMobile.Common.Controls;
using BitMobile.Common.DataAccessLayer;
using BitMobile.Common.Debugger;
using BitMobile.Common.Device.Providers;
using BitMobile.Common.ScriptEngine;
using BitMobile.Common.ValueStack;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitMobile.MobileClient.UnitTests.BusinessProcess
{
    [TestClass]
    public class DialogTests
    {
        private readonly Dialog _dialog;
        private readonly ScriptEngineMock _scriptEngineMock;
        private readonly ApplicationContextMock _applicationContextMock;

        public DialogTests()
        {
            _scriptEngineMock = new ScriptEngineMock();
            _applicationContextMock = new ApplicationContextMock();
            _dialog = new Dialog(_scriptEngineMock, _applicationContextMock);
        }

        [TestMethod]
        public void Alert_ThreeButtons_NeutralResponse()
        {
            var handler = new JsExecutableMock();
            _dialog.Alert("Hello", handler, 42, "Yes", "No", "Maybe");

            Assert.AreEqual(42, handler.State);
            Assert.AreEqual(2, handler.GetArgs<Args<int>>().Result);
        }

        class JsExecutableMock : IJsExecutable
        {
            public object State { get; private set; }

            public object Args { get; private set; }

            public T GetArgs<T>()
            {
                return (T)Args;
            }

            public void ExecuteStandalone(object visitor, params object[] parameters)
            {
                throw new NotImplementedException();
            }

            public void ExecuteCallback(object visitor, object state, object args)
            {
                State = state;
                Args = args;
            }
        }

        class ScriptEngineMock : IScriptEngine
        {
            public object Visitor { get; private set; }
            public IDebugger Debugger { get; private set; }
            public object CallFunction(string name, params object[] args)
            {
                throw new NotImplementedException();
            }

            public object CallFunctionNoException(string name, params object[] args)
            {
                throw new NotImplementedException();
            }

            public object CallVariable(string varName)
            {
                throw new NotImplementedException();
            }

            public void AddVariable(string name, object value)
            {
                throw new NotImplementedException();
            }

            public void ApplyBreakPoints()
            {
                throw new NotImplementedException();
            }

            public Exception CreateException(object innerObject)
            {
                throw new NotImplementedException();
            }
        }

        class ApplicationContextMock : IApplicationContext
        {
            public IConfiguration Configuration { get; private set; }
            public IValueStack ValueStack { get; private set; }
            public IDictionary<string, object> GlobalVariables { get; private set; }
            public IWorkflow Workflow { get; private set; }
            public IDal Dal { get; private set; }
            public IScreenData CurrentScreen { get; private set; }
            public string LocalStorage { get; private set; }
            public ILocationProvider LocationProvider { get; private set; }
            public ITracker LocationTracker { get; private set; }
            public IApplicationSettings Settings { get; private set; }
            public IGalleryProvider GalleryProvider { get; private set; }
            public ICameraProvider CameraProvider { get; private set; }
            public IDialogProvider DialogProvider { get; private set; }
            public IDisplayProvider DisplayProvider { get; private set; }
            public IClipboardProvider ClipboardProvider { get; private set; }
            public IEmailProvider EmailProvider { get; private set; }

            public bool OpenScreen(string screenName, string controllerName, Dictionary<string, object> parameters = null, bool isBackCommand = false,
                bool isRefresh = false)
            {
                throw new NotImplementedException();
            }

            public void RefreshScreen(Dictionary<string, object> parameters = null)
            {
                throw new NotImplementedException();
            }

            public void InvokeOnMainThread(Action action)
            {
                throw new NotImplementedException();
            }

            public void InvokeOnMainThreadSync(Action action)
            {
                throw new NotImplementedException();
            }

            public void HandleException(Exception e)
            {
                throw new NotImplementedException();
            }

            public bool Validate(string args)
            {
                throw new NotImplementedException();
            }

            public void PhoneCall(string number)
            {
                throw new NotImplementedException();
            }

            public void ScanBarcode(Action<object> callback)
            {
                throw new NotImplementedException();
            }

            public void Wait()
            {
                throw new NotImplementedException();
            }

            public void Exit(bool clearCache)
            {
                throw new NotImplementedException();
            }
        }
    }
}
