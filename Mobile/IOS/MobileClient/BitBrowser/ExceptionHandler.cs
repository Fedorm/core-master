using System;
using BitMobile.Utilities.LogManager;
using System.Collections.Generic;
using System.Reflection;
using MonoTouch.UIKit;
using System.IO;
using BitMobile.Utilities.Translator;
using BitMobile.Utilities.Exceptions;
using BitMobile.Application;
using BitMobile.DataAccessLayer;
using MonoTouch.MessageUI;
using MonoTouch.Foundation;

namespace BitMobile.IOS
{
	public class ExceptionHandler : CustomExceptionHandler
	{
		ApplicationSettings _settings;
		AppDelegate _application;

		public ExceptionHandler (ApplicationSettings settings, AppDelegate application)
		{			
			_settings = settings;
			_application = application;
		}

		#region implemented abstract members of CustomExceptionHandler

		protected override void SendLog (Log log, Action onSuccess, Action onFail)
		{
			if (_settings.DevelopersEmail != null)
                SendToEmail(EMAIL_TITLE, log.Text, log.Attachment, onSuccess, onFail);
			else {
				if (log.SendReport ())
					onSuccess ();
				else
					onFail ();
			}
		}

		protected override void PrepareException (Exception e, Action<Exception> next)
		{
			// TODO: Show message with information for developers in debug mode
			next (e);
		}

		protected override void ShutDownApplication ()
		{
			UIApplication.SharedApplication.PerformSelector (new MonoTouch.ObjCRuntime.Selector ("terminateWithSuccess"), null, 0f);
		}

		protected override void ShowDialog (string title, string message, Action<int> onClick, string cancelButtonTitle, params string[] otherButtons)
		{
			UIAlertView alert = new UIAlertView (title, message, null, cancelButtonTitle, otherButtons);
			alert.Clicked += (object sender, UIButtonEventArgs e) => {
				onClick (e.ButtonIndex);
			};
			alert.Show ();
		}

		protected override void PrepareLog (Log log)
		{
			log.UserName = "Sashka";
			log.Email = "unknown@unknown.ufo";
			log.Url = _settings.BaseUrl;
			log.DeviceId = ApplicationContext.UniqueID;

			log.OSTag = "ios";
			log.PlatformVersionTag = CoreInformation.CoreVersion.ToString ();

			if (_application.Context != null) {
				ApplicationContext context = _application.Context;

				if (context.Workflow != null) {
					var workflow = context.Workflow;
					log.CurrentWorkflow = workflow.Name;
					if (workflow.CurrentStep != null) {
						log.CurrentStep = workflow.CurrentStep.Name;

						log.CurrentScreen = workflow.CurrentStep.Screen;
						log.CurrentController = workflow.CurrentStep.Controller ?? workflow.Controller;
					}

				}
				if (context.DAL != null) {
					log.ConfigurationNameTag = context.DAL.ConfigName;
					log.ConfigurationVersionTag = context.DAL.ConfigVersion;

                    if (!string.IsNullOrWhiteSpace(context.DAL.UserEmail))
					    log.Email = context.DAL.UserEmail;
				}
			}

			log.Attachment = "<Info>";
			// settings
			string settings = "<Settings>";
			foreach (var property in typeof(Settings).GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
				object value = property.GetValue (this._settings);
				settings += string.Format ("<{0}>{1}</{0}>\r\n", property.Name, value ?? "null");
			}
			settings += "</Settings>";
			log.Attachment += settings;

			// device info
			string deviceInfo = "<DeviceInfo>";
			deviceInfo += string.Format ("<{0}>{1}</{0}>\r\n", "Description", UIDevice.CurrentDevice.Description.Replace ('<', '_').Replace ('>', '_'));
			deviceInfo += string.Format ("<{0}>{1}</{0}>\r\n", "IdentifierForVendor", UIDevice.CurrentDevice.IdentifierForVendor.ToString ().Replace ('<', '_').Replace ('>', '_'));          
			deviceInfo += string.Format ("<{0}>{1}</{0}>\r\n", "Model", UIDevice.CurrentDevice.Model);          
			deviceInfo += string.Format ("<{0}>{1}</{0}>\r\n", "Name", UIDevice.CurrentDevice.Name);          
			deviceInfo += string.Format ("<{0}>{1}</{0}>\r\n", "SystemName", UIDevice.CurrentDevice.SystemName);          
			deviceInfo += string.Format ("<{0}>{1}</{0}>\r\n", "SystemVersion", UIDevice.CurrentDevice.SystemVersion);          
			deviceInfo += "</DeviceInfo>";
			log.Attachment += deviceInfo;

			// value stack
			if (_application.Context != null && _application.Context.ValueStack != null) {
				string dump = _application.Context.ValueStack.GetContentString ();
				log.Attachment += dump;
			}

			log.Attachment += "</Info>";
		}

		#endregion

		void SendToEmail (string title, string text, string attachment, Action onSuccess, Action onFail)
		{
			if (MFMailComposeViewController.CanSendMail) {
				MFMailComposeViewController c = new MFMailComposeViewController ();
				c.SetSubject (title);
				c.SetToRecipients (new string[]{ _settings.DevelopersEmail });
				c.SetMessageBody (text, false);
				NSData data = new NSString (attachment).DataUsingEncoding (NSStringEncoding.UTF8);
				c.AddAttachmentData (data, "text/xml", "info.xml");
				_application.RootController.PresentViewController (c, true, null);
				c.Finished += (object sender, MFComposeResultEventArgs e) => {
					if (e.Result == MFMailComposeResult.Sent)
						onSuccess ();
					else
						onFail ();
					e.Controller.DismissViewController (true, null);
				};

			} else {
				UIAlertView alert = new UIAlertView ("Cannot send report", "E-mail messages not allowed", null, "OK");
				alert.Show ();
			}
		}
	}
}

