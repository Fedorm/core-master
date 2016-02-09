using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using BitMobile.Application.IO;
using BitMobile.Common.Device.Providers;
using BitMobile.Common.Log;
using BitMobile.Droid.Application;

namespace BitMobile.Droid.Providers
{
    class EmailProvider : IEmailProvider
    {
        private readonly Settings _settings;
        private readonly BaseScreen _activity;

        public EmailProvider(Settings settings, BaseScreen activity)
        {
            _settings = settings;
            _activity = activity;
        }

        const string EmailTitle = "Email";

        public async Task<bool> SendReport(IReport report)
        {
            var email = new Intent(Intent.ActionSend);
            email.PutExtra(Intent.ExtraSubject, EmailTitle);
            email.PutExtra(Intent.ExtraText, report.Body);
            email.PutExtra(Intent.ExtraEmail, new[] { _settings.DevelopersEmail });
            
            IOContext.Current.CreateDirectory(BitBrowserApp.Temp);

            string path = Path.Combine(BitBrowserApp.Temp, "info.xml");

            using (var stream = new FileStream(path, FileMode.OpenOrCreate
                , FileAccess.ReadWrite, FileShare.None))
            using (var writer = new StreamWriter(stream))
                writer.Write(report.Attachment);

            email.PutExtra(Intent.ExtraStream, Uri.Parse("file://" + path));
            email.SetType("plain/text");

            BaseScreen.ActivityResult result = await _activity.StartActivityForResultAsync(email);
            return result.Result == Result.Ok;
        }

        public async Task OpenEmailManager(string[] emails, string subject, string text, string[] attachments)
        {
            var email = new Intent(Intent.ActionSendMultiple);
            email.PutExtra(Intent.ExtraSubject, subject);
            email.PutExtra(Intent.ExtraText, text);
            email.PutExtra(Intent.ExtraEmail, emails);

            var uris = new List<IParcelable>();
            foreach (string attachment in attachments)
                if (!string.IsNullOrWhiteSpace(attachment))
                    using (var file = new Java.IO.File(attachment))
                    {
                        Uri uri = Uri.FromFile(file);
                        uris.Add(uri);
                    }

            email.PutParcelableArrayListExtra(Intent.ExtraStream, uris);
            email.SetType("plain/text");

            await _activity.StartActivityForResultAsync(email);
            // anyway result == Rasult.Cancelled
            uris.ForEach(val => val.Dispose());
        }
    }
}