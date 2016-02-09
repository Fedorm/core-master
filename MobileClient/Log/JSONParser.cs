using System;
using System.Collections.Generic;
using System.Web;

namespace BitMobile.Log
{
    static class JsonParser
    {
        public static Dictionary<string, string> ParseJson(string res)
        {
            string[] lines = System.Text.RegularExpressions.Regex.Split(res, "(?=,\")");
            var ht = new Dictionary<string, string>(20);
            var st = new Stack<string>(20);

            foreach (var line in lines)
            {
                var pair = line.Split(":".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);

                if (pair.Length == 2)
                {
                    var key = ClearString(pair[0]);
                    var val = ClearString(pair[1]);

                    if (val == "{")
                    {
                        st.Push(key);
                    }
                    else
                    {
                        if (st.Count > 0)
                        {
                            key = string.Join("_", st) + "_" + key;
                        }

                        if (ht.ContainsKey(key))
                        {
                            ht[key] += "&" + val;
                        }
                        else
                        {
                            ht.Add(key, val);
                        }
                    }
                }
                else if (line.IndexOf('}') != -1 && st.Count > 0)
                {
                    st.Pop();
                }
            }

            return ht;
        }
        private static string ClearString(string str)
        {
            str = str.Trim();
            if (str[0].Equals(',')) str = str.Remove(0, 1);
            var ind0 = str.IndexOf("\"", StringComparison.Ordinal);
            var ind1 = str.LastIndexOf("\"", StringComparison.Ordinal);

            if (ind0 != -1 && ind1 != -1)
            {
                str = str.Substring(ind0 + 1, ind1 - ind0 - 1);
            }
            else if (str[str.Length - 1] == ',')
            {
                str = str.Substring(0, str.Length - 1);
            }

            str = HttpUtility.UrlDecode(str);

            return str;
        }
    }

}