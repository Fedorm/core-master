using BitMobile.Utilities.Exceptions;
using BitMobile.Utilities.Translator;
using System;
using System.IO;
using System.Reflection;
using BitMobile.Controls;
using BitMobile.DataAccessLayer;
using BitMobile.Utilities.LogManager;
using Android.Content;

namespace BitMobile.Droid
{
    class ExceptionHandler : CustomExceptionHandler
    {
        readonly Settings _settings;
        readonly BaseScreen _screen;
        private readonly ApplicationContext _context;

        public ExceptionHandler(Settings settings, BaseScreen screen, ApplicationContext context)
        {
            _settings = settings;
            _screen = screen;
            _context = context;
        }

        protected override void SendLog(Log log, Action onSuccess, Action onFail)
        {
            if (_settings.DevelopersEmail != null)
            {
                var email = new Intent(Intent.ActionSend);
                email.PutExtra(Intent.ExtraSubject, EMAIL_TITLE);
                email.PutExtra(Intent.ExtraText, log.Text);
                email.PutExtra(Intent.ExtraEmail, new[] { _settings.DevelopersEmail });

                if (!Directory.Exists(BitBrowserApp.Temp))
                    Directory.CreateDirectory(BitBrowserApp.Temp);
                string path = Path.Combine(BitBrowserApp.Temp, "info.xml");

                using (var stream = new FileStream(path, FileMode.OpenOrCreate
                    , FileAccess.ReadWrite, FileShare.None))
                using (var writer = new StreamWriter(stream))
                    writer.Write(log.Attachment);

                email.PutExtra(Intent.ExtraStream, Android.Net.Uri.Parse("file://" + path));
                email.SetType("plain/text");

                _screen.StartActivityForResult(email
                    , BaseScreen.ReportEmailRequestCode
                    , (resultCode, data) => onSuccess());
            }
            else
            {
                if (log.SendReport())
                    onSuccess();
                else
                    onFail();
            }
        }

        protected override void PrepareException(Exception e, Action<Exception> next)
        {
            if (Application.ApplicationContext.Context.Settings.DevelopModeEnabled)
            {
                string title = D.APP_HAS_BEEN_INTERRUPTED;
                var ce = e as CustomException;
                if (ce != null)
                    title = ce.FriendlyMessage;

                ShowDialog(title, e.Message, index => next(e), D.OK, null);
            }
            else
                next(e);
        }

        protected override void ShutDownApplication()
        {
            _screen.Finish();
        }

        protected override void ShowDialog(string title, string message, Action<int> onClick, string cancelButtonTitle, params string[] otherButtons)
        {
            var buttons = new DialogButton<object>[2];

            buttons[0] = new DialogButton<object>(cancelButtonTitle, (state, result) => onClick(0));
            if (otherButtons != null)
                buttons[1] = new DialogButton<object>(otherButtons[0], (state, result) => onClick(1));

            _context.DialogProviderInternal.ShowDialog(title, message, buttons[0], buttons[1]);
        }

        protected override void PrepareLog(Log log)
        {
            log.UserName = _settings.UserName;
            log.Email = "unknown@unknown.ufo"; // TODO: добавить почту юзера
            log.Url = _settings.BaseUrl;
            log.DeviceId = BitBrowserApp.Current.DeviceId;

            log.OSTag = "android";
            log.PlatformVersionTag = CoreInformation.CoreVersion.ToString();

            if (BitBrowserApp.Current.AppContext != null)
            {
                var context = BitBrowserApp.Current.AppContext;
                if (context.Workflow != null)
                {
                    var workflow = context.Workflow;
                    log.CurrentWorkflow = workflow.Name;
                    if (workflow.CurrentStep != null)
                    {
                        log.CurrentStep = workflow.CurrentStep.Name;

                        log.CurrentScreen = workflow.CurrentStep.Screen;
                        log.CurrentController = workflow.CurrentStep.Controller ?? workflow.Controller;
                    }

                }
                if (context.DAL != null)
                {
                    log.ConfigurationNameTag = context.DAL.ConfigName;
                    log.ConfigurationVersionTag = context.DAL.ConfigVersion;

                    if (!string.IsNullOrWhiteSpace(context.DAL.UserEmail))
                        log.Email = context.DAL.UserEmail;
                }
            }

            log.Attachment = "<Info>";
            // settings
            string settings = "<Settings>";
            foreach (var property in typeof(Settings).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                object value = property.GetValue(_settings);
                settings += string.Format("<{0}>{1}</{0}>\r\n", property.Name, value ?? "null");
            }
            settings += "</Settings>";
            log.Attachment += settings;

            // device info
            string deviceInfo = "<DeviceInfo>";
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "Board", Android.OS.Build.Board);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "Bootloader", Android.OS.Build.Bootloader);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "Brand", Android.OS.Build.Brand);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "CpuAbi", Android.OS.Build.CpuAbi);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "CpuAbi2", Android.OS.Build.CpuAbi2);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "Device", Android.OS.Build.Device);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "Display", Android.OS.Build.Display);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "Fingerprint", Android.OS.Build.Fingerprint);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "Hardware", Android.OS.Build.Hardware);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "Host", Android.OS.Build.Host);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "Id", Android.OS.Build.Id);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "Manufacturer", Android.OS.Build.Manufacturer);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "Model", Android.OS.Build.Model);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "Product", Android.OS.Build.Product);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "Radio", Android.OS.Build.Radio);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "Tags", Android.OS.Build.Tags);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "Time", Android.OS.Build.Time);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "Type", Android.OS.Build.Type);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "User", Android.OS.Build.User);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "VERSION.Codename", Android.OS.Build.VERSION.Codename);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "VERSION.Incremental", Android.OS.Build.VERSION.Incremental);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "VERSION.Release", Android.OS.Build.VERSION.Release);
            deviceInfo += string.Format("<{0}>{1}</{0}>\r\n", "VERSION.Sdk", Android.OS.Build.VERSION.Sdk);
            deviceInfo += "</DeviceInfo>";
            log.Attachment += deviceInfo;

            // value stack
            if (BitBrowserApp.Current.AppContext != null
                && BitBrowserApp.Current.AppContext.ValueStack != null)
            {
                string dump = BitBrowserApp.Current.AppContext.ValueStack.GetContentString();
                log.Attachment += dump;
            }

            log.Attachment += "</Info>";
        }
    }
}
