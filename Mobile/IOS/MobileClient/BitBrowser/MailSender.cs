using System;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace BitMobile.IOS
{
	public static class MailSender
	{
		public static void Send(String msg)
		{
			MailMessage mm = new MailMessage();
			mm.To.Add(new MailAddress("serg@psoftware.ru"));
			mm.From = new MailAddress("polevi@mail.ru");

			mm.Subject = "bug report";
			mm.Body = msg;
			mm.BodyEncoding = System.Text.UTF8Encoding.UTF8;
			mm.DeliveryNotificationOptions = DeliveryNotificationOptions.Never;

			SmtpClient ss = new SmtpClient("smtp.gmail.com");
			ss.UseDefaultCredentials = false;
			ss.EnableSsl = true;
			ss.Credentials = new NetworkCredential("serg@psoftware.ru","wcmcyt0o");
			ss.Port = 587;

			ServicePointManager.ServerCertificateValidationCallback = delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
				return true;
			};

			ss.Send(mm);
		}
	}
}

