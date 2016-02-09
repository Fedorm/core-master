using CodeFactory.DatabaseFactory;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Xml;
using PushSharp;
using PushSharp.Android;
using PushSharp.Apple;

namespace PushService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class PushService : IPushServiceRequestHandler
    {
        private const string IosDevelopmentCertificate = "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";
        //private const string IosProductionCertificate =  "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";
        private const string IosProductionCertificate = "MIIFjjCCBHagAwIBAgIIS06//Xvs+iAwDQYJKoZIhvcNAQEFBQAwgZYxCzAJBgNVBAYTAlVTMRMwEQYDVQQKDApBcHBsZSBJbmMuMSwwKgYDVQQLDCNBcHBsZSBXb3JsZHdpZGUgRGV2ZWxvcGVyIFJlbGF0aW9uczFEMEIGA1UEAww7QXBwbGUgV29ybGR3aWRlIERldmVsb3BlciBSZWxhdGlvbnMgQ2VydGlmaWNhdGlvbiBBdXRob3JpdHkwHhcNMTUwNjE4MTIwODU5WhcNMTYwNjE3MTIwODU5WjCBjTElMCMGCgmSJomT8ixkAQEMFXJ1LmZpcnN0Yml0LmJpdG1vYmlsZTFCMEAGA1UEAww5QXBwbGUgUHJvZHVjdGlvbiBJT1MgUHVzaCBTZXJ2aWNlczogcnUuZmlyc3RiaXQuYml0bW9iaWxlMRMwEQYDVQQLDAo3TFg2QU0zNzNMMQswCQYDVQQGEwJVUzCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBANIrDzW+skTHBimPk7RsdmW1lkDvCo/3STuD4kapug6/oChQ+yOXqk7HQiOTE/yyMjrjKX5hwtRocDS716ESUQitbnu+EH3fiYIELUpbgL/v6w3b1yTKLR3jj3eQ/WR1G05f3MmojT2kKYePEH9A/5kRM38weS8k+VLHQu5HBGmtH6w3KIVRwB6Hm7FfKq3RWxQkctOTowbHkbIlIpU0N3Cl4LGjNsCJHmNQlRWFRliWNNMULAz6dboy7owAxpDU/tOhgOn7NCy11VVcNVZoGXq7Rgsksc4rzeduI0l8aewiYuoiPqLYkE7oqTzDRRicP4kWzJavXVflDkzAUCPICtkCAwEAAaOCAeUwggHhMB0GA1UdDgQWBBSs/qcuZBGVZvLjMjw205gCf4rbrDAJBgNVHRMEAjAAMB8GA1UdIwQYMBaAFIgnFwmpthhgi+zruvZHWcVSVKO3MIIBDwYDVR0gBIIBBjCCAQIwgf8GCSqGSIb3Y2QFATCB8TCBwwYIKwYBBQUHAgIwgbYMgbNSZWxpYW5jZSBvbiB0aGlzIGNlcnRpZmljYXRlIGJ5IGFueSBwYXJ0eSBhc3N1bWVzIGFjY2VwdGFuY2Ugb2YgdGhlIHRoZW4gYXBwbGljYWJsZSBzdGFuZGFyZCB0ZXJtcyBhbmQgY29uZGl0aW9ucyBvZiB1c2UsIGNlcnRpZmljYXRlIHBvbGljeSBhbmQgY2VydGlmaWNhdGlvbiBwcmFjdGljZSBzdGF0ZW1lbnRzLjApBggrBgEFBQcCARYdaHR0cDovL3d3dy5hcHBsZS5jb20vYXBwbGVjYS8wTQYDVR0fBEYwRDBCoECgPoY8aHR0cDovL2RldmVsb3Blci5hcHBsZS5jb20vY2VydGlmaWNhdGlvbmF1dGhvcml0eS93d2RyY2EuY3JsMAsGA1UdDwQEAwIHgDATBgNVHSUEDDAKBggrBgEFBQcDAjAQBgoqhkiG92NkBgMCBAIFADANBgkqhkiG9w0BAQUFAAOCAQEAvlm/S8FRbQ8bEsY4t0kV501nTdFLmIageORNLDILxOMvvqUDUR76Mwv1yTZU/PHFjMmXAnT/hHaIipDcDhGgLZ6Pdf5/jP3WsXr5FeCgEM3T1c8q9uiHzPt48sQ6PptyJyq+Spp5JTHAjmlo8lvz/I4uV+SKhVTJbJKUPw2wQhTBHSc0tn2BdGFY9whSDVrzRYB7x3vpskwe4oC3P1exXsZJsZBEUlT/14Cg11WFQbqSu+pYpazthNMwcyihb+ux5/vBtciA7Ielj0lsZCI00Aw+KK2RUpUxk/wPmUFo77q8x0DhMdap19aI/DC7GvORI7q6YESoDrZZoEHWSPuwEw==";

        private const String AndroidProjectId = "293034226758";
        private const String AndroidServerKey = "AIzaSyCZZ84s0wQPq734rBPCsCThkdlyHRubvRg";

        private readonly Common.Solution _solution;
        private readonly NetworkCredential _credential;

        public PushService(String scope, NetworkCredential credential)
        {
            if (credential.UserName.ToLower().Equals("admin"))
                Common.Logon.CheckAdminCredential(scope, credential);
            else
                Common.Logon.CheckUserCredential(scope, credential);
            _solution = Common.Solution.CreateFromContext(scope);
            _credential = credential;
        }

        public Stream GetAndroidProjectId()
        {
            if (_credential.UserName.ToLower().Equals("admin"))
                return Common.Utils.MakeTextAnswer("Operation is not allowed");

            return Common.Utils.MakeTextAnswer(AndroidProjectId);
        }

        public Stream RegisterDevice()
        {
            try
            {
                IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                string deviceId = request.Headers["deviceId"];
                string os = request.Headers["os"];
                string package = request.Headers["package"];
                string token = request.Headers["token"];


                if (string.IsNullOrEmpty(deviceId) || string.IsNullOrEmpty(token))
                    return Common.Utils.MakeTextAnswer("bad request");

                using (var conn = new SqlConnection(_solution.ConnectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand("[admin].[RegisterPushToken]", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    try
                    {
                        SqlCommandBuilder.DeriveParameters(cmd);
                    }
                    catch
                    {
                        try
                        {
                            TryUpdateDatabase();
                            SqlCommandBuilder.DeriveParameters(cmd);
                        }
                        catch
                        {
                            throw new Exception("Database does not support push notifications");
                        }
                    }
                    cmd.Parameters["@UserId"].Value = Guid.Parse(_credential.UserName);
                    cmd.Parameters["@DeviceId"].Value = deviceId;
                    cmd.Parameters["@OS"].Value = os;
                    cmd.Parameters["@Package"].Value = package;
                    cmd.Parameters["@Token"].Value = token;
                    cmd.ExecuteNonQuery();

                    return Common.Utils.MakeTextAnswer("ok");
                }
            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e);
            }
        }

        public Stream SendMessage(Stream messageBody)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(messageBody);

                var msg = new PushMessage(doc, _solution.Name);
                var push = new PushBroker();

                string result = "";

                if (msg.HasGcmRecipients)
                {
                    push.RegisterGcmService(new GcmPushChannelSettings(AndroidServerKey));

                    string json = "{\"alert\":\"" + msg.Data + "\"}";
                    foreach (string token in msg.GcmTokens)
                        push.QueueNotification(new GcmNotification().ForDeviceRegistrationId(token).WithJson(json));
                    result += "Android: ok; ";
                }

                if (msg.HasApnsRecipients)
                {
                    var appleCert = Convert.FromBase64String(IosProductionCertificate);
                    push.RegisterAppleService(new ApplePushChannelSettings(false, appleCert, "h2o+c2h5oh"));
                    foreach (string token in msg.ApnsTokens)
                    {
                        push.QueueNotification(new AppleNotification()
                            .ForDeviceToken(token)
                            .WithAlert(msg.Data)
                            .WithSound("sound.caf"));
                    }

                    result += "IOS: ok; ";
                }

                return Common.Utils.MakeTextAnswer(result);

            }
            catch (Exception e)
            {
                return Common.Utils.MakeExceptionAnswer(e);
            }
        }

        private void TryUpdateDatabase()
        {
            Assembly a = Assembly.GetExecutingAssembly();
            //run script
            var dbf = new DatabaseFactory(_solution.ConnectionString);
            String script = new StreamReader(a.GetManifestResourceStream("PushService.Database.database.sql")).ReadToEnd();
            dbf.RunScript(script);
        }
    }
}
