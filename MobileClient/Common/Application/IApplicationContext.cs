using System;
using System.Collections.Generic;
using BitMobile.Common.Application.Tracking;
using BitMobile.Common.BusinessProcess.SolutionConfiguration;
using BitMobile.Common.BusinessProcess.WorkingProcess;
using BitMobile.Common.Controls;
using BitMobile.Common.DataAccessLayer;
using BitMobile.Common.Device.Providers;
using BitMobile.Common.ValueStack;

namespace BitMobile.Common.Application
{
    public interface IApplicationContext
    {
        IConfiguration Configuration { get; }
        IValueStack ValueStack { get; }
        IDictionary<string, object> GlobalVariables { get; }
        IWorkflow Workflow { get; }
        IDal Dal { get; }
        IScreenData CurrentScreen { get; }
        string LocalStorage { get; }
        ILocationProvider LocationProvider { get; }
        ITracker LocationTracker { get; }
        IApplicationSettings Settings { get; }
        IGalleryProvider GalleryProvider { get; }
        ICameraProvider CameraProvider { get; }
        IDialogProvider DialogProvider { get; }
        IDisplayProvider DisplayProvider { get; }
        IClipboardProvider ClipboardProvider { get; }
        IEmailProvider EmailProvider { get; }
        ILocalNotificationProvider LocalNotificationProvider { get; }
        IWebProvider WebProvider { get; }
        bool InBackground { get; }

        event Action ApplicationBackground;
        event Action ApplicationRestore;

        bool OpenScreen(String screenName, String controllerName, Dictionary<String, object> parameters = null, bool isBackCommand = false, bool isRefresh = false);
        void RefreshScreen(Dictionary<String, object> parameters = null);
        void InvokeOnMainThread(Action action);
        void InvokeOnMainThreadSync(Action action);
        void HandleException(Exception e);
        bool Validate(string args);

        // TODO: Добавить что то типа: NativeFunctions
        void PhoneCall(string number);
        void ScanBarcode(Action<object> callback);

        void Wait();
        void Exit(bool clearCache);
    }
}

