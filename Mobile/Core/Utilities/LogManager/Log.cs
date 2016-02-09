using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace BitMobile.Utilities.LogManager
{
    public class Log
    {
        const int ATTEMPTS_NUMBER = 10;

        public string Text { get; set; }
        public bool IsCrash { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }

        public string CurrentWorkflow { get; set; }
        public string CurrentStep { get; set; }
        public string CurrentScreen { get; set; }
        public string CurrentController { get; set; }
        public string Url { get; set; }
        public string DeviceId { get; set; }

        public string OSTag { get; set; }
        public string PlatformVersionTag { get; set; }
        public string ConfigurationNameTag { get; set; }
        public string ConfigurationVersionTag { get; set; }

        public string Attachment { get; set; }

		public bool SendReport()
        {
            bool sent = false;
            int attempts = 0;

            string title = PrepareTitle();
            string text = PrepareText();
            List<string> tags = new List<string>(4);
            tags.Add(OSTag);
            tags.Add("p:" + PlatformVersionTag);
            if (!string.IsNullOrWhiteSpace(ConfigurationNameTag))
                tags.Add("c:" + ConfigurationNameTag);
            if (!string.IsNullOrWhiteSpace(ConfigurationVersionTag))
                tags.Add("c:" + ConfigurationVersionTag);
			           
            do
            {
                try
                {
					Zendesk.CreateTicket(title, text, UserName, Email, IsCrash, tags.ToArray(), Attachment);
                    sent = true;
                }
                catch (WebException e)
                {
					try 
					{
						if (e.Response != null) {
							Stream stream = e.Response.GetResponseStream();
							StreamReader reader = new StreamReader(stream);
							string str = reader.ReadToEnd();
							Console.WriteLine(str);
						}
					} 
					catch {	}
                    
                }
                catch (Exception e)
                {                                    
                }
                finally
                {
                    attempts++;
                }
            } while (!sent && attempts < ATTEMPTS_NUMBER);

            return sent;
        }

        string PrepareTitle()
        {
            string line = this.Text
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0];

            // to remove namespace of exceptions
            // for example: BitMobile.Utilities.Exceptions.NonFatalException ===> NonFatalException:
            int lastDot = -1;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '.')
                    lastDot = i;
                if (line[i] == ':')
                {
                    if (lastDot > 0)
                        line = line.Substring(lastDot + 1);
                    break;
                }
            }

            return line;
        }

        string PrepareText()
        {
            string result = string.Format(
                "Url: {1} {0}Device ID: {2} {0}Workflow: {3} {0}Step: {4} {0}Screen: {5} {0}Controller: {6} {0}"
                , Environment.NewLine, Url, DeviceId, CurrentWorkflow, CurrentStep, CurrentScreen, CurrentController);
            result += Environment.NewLine;
            result += this.Text.ToString();
            return result;
        }
    }
}
