using BitMobile.Utilities.Exceptions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace BitMobile.DataAccessLayer
{
    public partial class DAL
    {
        private bool translationProcessed = false;
        private Dictionary<String, String> translation = null;

		public string TranslateString(String s)
		{
			if (translation == null)
				InitTranslation ();

			Regex re = new Regex(@"#(?<key>\w+)#");
			foreach (Match m in re.Matches(s))
			{
				String key = m.Groups["key"].Value;
				if (translation.ContainsKey(key))
					s = s.Replace(m.Value, translation[key]);
			}
			return s;
		}

        public System.IO.Stream GetScreenByName(String screenName)
        {
			return GetResource("Screen", appName, screenName);            
        }

        public Stream GetConfiguration()
        {
            return GetResource("Configuration", appName, null);
        }

        public Stream GetBusinessProcess(String bpName)
        {
            return GetResource("BusinessProcess", appName, bpName);
        }

        public Stream GetBusinessProcess2(String bpName)
        {
            return GetResource("BusinessProcess", appName, bpName);
        }

        public bool TryGetStyleByName(String styleName, out Stream style)
        {
            return TryGetResource("Style", appName, styleName, out style);
        }

        public Stream GetImageByName(String imagePath)
        {
            return GetResource("Image", appName, imagePath);
        }

        public bool TryGetScriptByName(String scriptPath, out Stream script)
        {
            return TryGetResource("Script", appName, scriptPath, out script);
        }

        Stream GetResource(String resType, String app, String resName)
        {
            Stream result;
            if (!TryGetResource(resType, app, resName, out result))
                throw new ResourceNotFoundException(resType, resName);
            return result;
        }

        bool TryGetResource(String resType, String app, String resName, out Stream resource)
        {
            BitMobile.DbEngine.Database db = BitMobile.DbEngine.Database.Current;

            if (String.IsNullOrEmpty(resName))
            {
                resource = db.SelectStream(String.Format("SELECT Data FROM resource_{0} WHERE Name LIKE '{1}/%'", resType, app));
            }
            else
            {
                String qry = String.Format("SELECT Data FROM resource_{0} WHERE Name = @p1", resType);
                String[] arr = resName.Split(new String[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length == 1)
                    resource = db.SelectStream(qry, String.Format("{0}/{1}", app, arr[arr.Length - 1]));
                else
                {
                    qry = qry + " AND Parent = @p2";
                    String parent = "";
                    for (int i = 0; i < arr.Length - 1; i++)
                        parent = "\\" + arr[i];
                    resource = db.SelectStream(qry, String.Format("{0}/{1}", app, arr[arr.Length - 1]), parent);
                }
            }

            return resource != null;
        }

		Dictionary<String, String> GetTranslationByName(String translationName)
		{
			Dictionary<String, String> result = null;

			Stream str;
			if (TryGetResource("Translation", appName, translationName, out str))
			{
				Dictionary<String, String> dict = new Dictionary<string, string>();
				System.IO.StreamReader r = new System.IO.StreamReader(str);
				while (!r.EndOfStream)
				{
					String line = r.ReadLine();
					if (!String.IsNullOrEmpty(line))
					{
						String[] arr = line.Trim().Replace('\t', '\r').Split(new String[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
						if (arr.Length == 2)
						{
							String key = arr[0];
							String value = arr[1];
							if (!dict.ContainsKey(key))
								dict.Add(key, value);
						}
					}
				}

				result = dict;
			}

			return result;
		}

		void InitTranslation()
		{
			this.translation = GetTranslationByName(String.Format("{0}.txt", language));
			if (translation == null)
			{
				this.translation = GetTranslationByName("en.txt");
				if (translation == null)
					throw new ResourceNotFoundException("language", language);
			}
			translationProcessed = true;
		}
    }
}

