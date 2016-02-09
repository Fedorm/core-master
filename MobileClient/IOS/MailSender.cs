using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace BitMobile.IOS
{
    public static class MailSender
    {
        public static void Send(String msg)
        {
            var mm = new MailMessage();
            mm.To.Add(new MailAddress("serg@psoftware.ru"));
            mm.From = new MailAddress("polevi@mail.ru");

            mm.Subject = "bug report";
            mm.Body = msg;
            mm.BodyEncoding = Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

            var ss = new SmtpClient("smtp.gmail.com");
            ss.UseDefaultCredentials = false;
            ss.EnableSsl = true;
            ss.Credentials = new NetworkCredential("serg@psoftware.ru", "wcmcyt0o");
            ss.Port = 587;

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            ss.Send(mm);
        }
    }
}