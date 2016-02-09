using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using BitMobile.Application.DbEngine;
using BitMobile.Application.Exceptions;
using BitMobile.Common.DbEngine;

namespace BitMobile.DataAccessLayer
{
    public partial class Dal
    {
        private readonly Dictionary<string, string> _translationCache = new Dictionary<string, string>();
        private Dictionary<String, String> _translation;

        public string TranslateString(string s)
        {
            if (_translation == null)
                InitTranslation();
            
            if (string.IsNullOrWhiteSpace(s))
                return s;

            string result;
            if (_translationCache.TryGetValue(s, out result))
                return result;

            if (TranslateStringInternal(_translation, s, out result))
                _translationCache.Add(s, result);
            
            return result;
        }

        public void ClearStringCache()
        {
            _translationCache.Clear();
        }

        public string TranslateByKey(string key)
        {
            if (_translation == null)
                InitTranslation();

            string result;
            if (_translation != null && _translation.TryGetValue(key, out result))
                return result;
            return key;
        }

        internal static bool TranslateStringInternal(IDictionary<string, string> translation, string s, out string result)
        {
            StringBuilder builder = null;
            bool inScope = false;
            int scopeStart = 0; // in string: 012#456#89 scopes is: 012, 456, 89; scopeStart = 0, 4, 8
            int i;
            for (i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '#')
                {
                    if (builder == null)
                        builder = new StringBuilder(s.Length);

                    if (inScope)
                    {
                        string key = s.Substring(scopeStart, i - scopeStart);
                        string value;
                        if (translation.TryGetValue(key, out value))
                            builder.Append(value);
                        else
                            builder.Append(s, scopeStart - 1, i - scopeStart + 2);

                        inScope = false;
                    }
                    else
                    {
                        builder.Append(s, scopeStart, i - scopeStart);
                        inScope = true;
                    }
                    scopeStart = i + 1;
                }
            }

            if (builder != null)
            {
                if (scopeStart != i)
                    builder.Append(s, scopeStart, i - scopeStart);

                result = builder.ToString();
                return true;
            }
            result = s;
            return false;
        }

        public Stream GetScreenByName(String screenName)
        {
            return GetResource("Screen", _appName, screenName);
        }

        public Stream GetConfiguration()
        {
            return GetResource("Configuration", _appName, null);
        }

        public Stream GetBusinessProcess(String bpName)
        {
            return GetResource("BusinessProcess", _appName, bpName);
        }

        public Stream GetBusinessProcess2(String bpName)
        {
            return GetResource("BusinessProcess", _appName, bpName);
        }

        public bool TryGetStyleByName(String styleName, out Stream style)
        {
            return TryGetResource("Style", _appName, styleName, out style);
        }

        public Stream GetImageByName(String imagePath)
        {
            return GetResource("Image", _appName, imagePath);
        }

        public bool TryGetScriptByName(String scriptPath, out Stream script)
        {
            return TryGetResource("Script", _appName, scriptPath, out script);
        }

        public string[] GetResources(string type)
        {
            string q = string.Format("SELECT Name, Parent FROM resource_{0}", type);
            IDataReader reader = DbContext.Current.Database.Select(q);
            var result = new List<string>();
            while (reader.Read())
            {
                string name = reader.GetString(0);
                name = name.Substring(_appName.Length + 1);// app/Main.js => Main.js
                string parent = reader.GetString(1);
                result.Add(Path.Combine(parent, name));
            }
            return result.ToArray();
        }

        private Stream GetResource(string resType, string app, string resName)
        {
            Stream result;
            if (!TryGetResource(resType, app, resName, out result))
                throw new ResourceNotFoundException(resType, resName);
            return result;
        }

        private bool TryGetResource(string resType, string app, String resName, out Stream resource)
        {
            IDatabase db = DbContext.Current.Database;

            if (string.IsNullOrEmpty(resName)) // for example: BusinessProcess
            {
                resource = db.SelectStream(String.Format("SELECT Data FROM resource_{0} WHERE Name LIKE '{1}/%'", resType, app));
            }
            else
            {
                string qry = string.Format("SELECT Data FROM resource_{0} WHERE Name = @p1", resType);
                string[] arr = resName.Split(new[] { "\\", "/" }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length == 1)
                    resource = db.SelectStream(qry, string.Format("{0}/{1}", app, arr[arr.Length - 1]));
                else
                {
                    qry = qry + " AND Parent = @p2";
                    string parent = "";
                    for (int i = 0; i < arr.Length - 1; i++)
                        parent = "\\" + arr[i];
                    resource = db.SelectStream(qry, string.Format("{0}/{1}", app, arr[arr.Length - 1]), parent);
                }
            }

            return resource != null;
        }

        private Dictionary<String, String> GetTranslationByName(String translationName)
        {
            Dictionary<String, String> dict = null;

            Stream str;
            if (TryGetResource("Translation", _appName, translationName, out str))
            {
                dict = new Dictionary<string, string>();
                var r = new StreamReader(str);
                while (!r.EndOfStream)
                {
                    string line = r.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        string[] arr = line.Trim().Replace('\t', '\r').Split(new[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
                        if (arr.Length == 2)
                        {
                            string key = arr[0];
                            string value = arr[1];
                            if (!dict.ContainsKey(key))
                                dict.Add(key, value);
                        }
                    }
                }
            }

            return dict;
        }

        private void InitTranslation()
        {
            _translation = GetTranslationByName(string.Format("{0}.txt", _language));
            if (_translation == null)
            {
                _translation = GetTranslationByName("en.txt");
                if (_translation == null)
                    throw new ResourceNotFoundException("language", _language);
            }
        }
    }
}

