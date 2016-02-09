using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;

namespace BitMobile.StyleSheet.Cache
{
    static class StyleSheetCache
    {
        private static readonly Dictionary<string, uint> StringsKeys = new Dictionary<string, uint>();
        private static uint _maxKey = 1;

        static StyleSheetCache()
        {
            StyleNames = new Dictionary<string, Type>();
            TagNames = new Dictionary<string, Type>();
            BuildNames();
        }

        public static Dictionary<string, Type> StyleNames { get; private set; }

        public static Dictionary<string, Type> TagNames { get; private set; }

        public static uint GetStringKey(string str)
        {
            if (str == null)
                return 0;

            uint key;
            if (!StringsKeys.TryGetValue(str, out key))
            {
                key = _maxKey;
                StringsKeys.Add(str, key);
                _maxKey++;
                return key;
            }
            return key;
        }

        private static void BuildNames()
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in a.GetTypes())
                {
                    if (type.Namespace != null)
                    {
                        if (IsStyleType(type))
                        {
                            string s = type.Name.ToLower();
                            object[] attr = type.GetCustomAttributes(typeof(SynonymAttribute), false);
                            if (attr.Length == 1)
                                s = ((SynonymAttribute)attr[0]).Name;
                            StyleNames.Add(s, type);
                        }

                        var markupElementAttribute = type.GetCustomAttribute<MarkupElementAttribute>(false);
                        if (markupElementAttribute != null && type.GetInterfaces().Contains(typeof(IStyledObject)))
                        {
                            string s = type.Name.ToLower();
                            object[] attr = type.GetCustomAttributes(typeof(SynonymAttribute), false);
                            if (attr.Length == 1)
                                s = ((SynonymAttribute)attr[0]).Name;
                            TagNames.Add(s, type);
                        }
                    }
                }

            }
        }

        private static bool IsStyleType(Type t)
        {
            return !t.IsAbstract && t.IsClass && t.GetInterfaces().Contains(typeof(IStyle));
        }
    }
}