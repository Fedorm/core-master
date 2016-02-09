using BitMobile.BusinessProcess;
using BitMobile.Common;
using BitMobile.Controls;
using BitMobile.DataAccessLayer;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BitMobile.Application
{
    public interface IApplicationContext
    {
        BitMobile.Configuration.Configuration Configuration { get; }
        ValueStack.ValueStack ValueStack { get; }
        IDictionary<string, object> GlobalVariables { get; }
        Workflow Workflow { get; }
        DAL DAL { get; }
        ScreenData CurrentScreen { get; }
        string LocalStorage { get; }
        ILocationProvider LocationProvider { get; }
        Tracker LocationTracker { get; }
        ApplicationSettings Settings { get; }
        IGalleryProvider GalleryProvider { get; }
        ICameraProvider CameraProvider { get; }
        IDialogProvider DialogProvider { get; }
        IClipboardProvider ClipboardProvider { get; }

        bool OpenScreen(String screenName, String controllerName, Dictionary<String, object> parameters = null, bool isBackCommand = false, bool isRefresh = false);
        void RefreshScreen(Dictionary<String, object> parameters = null);
        void InvokeOnMainThread(System.Action action);
        void InvokeOnMainThreadSync(System.Action action);
        void HandleException(Exception e);
        bool Validate(string args);

        // TODO: Добавить что то типа: NativeFunctions
        void PhoneCall(string number);
        void ScanBarcode(Action<object> callback);

        void Wait();
        void Exit(bool clearCache);
    }
}

