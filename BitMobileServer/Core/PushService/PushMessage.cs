using System;
using System.Collections.Generic;
using System.Xml;

namespace PushService
{
    class PushMessage
    {
        private readonly String _data;
        readonly IDictionary<string, IList<string>> _recipients;

        public PushMessage(XmlDocument doc, string dbName)
        {
            if (doc.DocumentElement == null)
                throw new NullReferenceException("doc.DocumentElement is null");

            var textNode = doc.DocumentElement.SelectSingleNode("//Message/Data");
            if (textNode == null)
                throw new Exception("Invalid message format");

            _data = textNode.InnerText;

            if (String.IsNullOrEmpty(_data))
                throw new Exception("Data is empty");
            if (_data.Length > 150)
                throw new Exception("Data length exceeds 150 bytes");

            var nodes = doc.DocumentElement.SelectNodes("//Message/Recipients");
            if (nodes == null)
                throw new Exception("Invalid message format");

            _recipients = new Dictionary<string, IList<string>>();
            foreach (XmlNode n in nodes)
            {
                Guid id;
                if (!Guid.TryParse(n.InnerText, out id))
                    throw new Exception("Invalid recipient Id");

                IDictionary<string, IList<string>> tokensByOs = Common.Logon.GetUserPushTokensByOs(dbName, id);
                AddRecipients(tokensByOs);
            }

            if (_recipients.Count == 0)
                throw new Exception("No recipients defined");
        }

        public bool HasGcmRecipients
        {
            get { return _recipients.ContainsKey("android"); }
        }

        public bool HasApnsRecipients
        {
            get { return _recipients.ContainsKey("ios"); }
        }

        public IEnumerable<string> ApnsTokens
        {
            get
            {
                IList<string> result;
                if (!_recipients.TryGetValue("ios", out result))
                    result = new List<string>();
                return result;
            }
        }

        public IEnumerable<string> GcmTokens
        {
            get
            {
                IList<string> result;
                if (!_recipients.TryGetValue("android", out result))
                    result = new List<string>();
                return result;
            }
        }

        public string Data
        {
            get { return _data; }
        }

        private void AddRecipients(IEnumerable<KeyValuePair<string, IList<string>>> tokensByOs)
        {
            foreach (var pair in tokensByOs)
            {
                IList<string> tokens;
                if (!_recipients.TryGetValue(pair.Key, out tokens))
                    _recipients.Add(pair.Key, tokens = new List<string>());
                foreach (string token in pair.Value)
                    if (!tokens.Contains(token))
                        tokens.Add(token);
            }
        }
    }
}
