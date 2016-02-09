using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BitMobile.Controls.StyleSheet
{
    public abstract class StyleSheet
    {
        static Dictionary<String, Type> styleNames = new Dictionary<string, Type>();
        static Dictionary<String, Type> tagNames = new Dictionary<string, Type>();

        static StyleSheet()
        {
            BuildNames();
        }

        public StyleSheet()
        {
            this.Styles = new Dictionary<string, List<Style>>();
        }

        public IStyleSheetHelper Helper { get; protected set; }

        public T GetHelper<T>() where T : IStyleSheetHelper
        {
            return (T)Helper;
        }

        public abstract Dictionary<Type, Style> GetStyles(object obj);

//        public abstract void ProcessChildren(object obj);

        public abstract void Assign(object root);

        public void Load(Stream stream)
        {
            string expression = ReadStream(stream);
            String[] expressions = expression.Split('}');
            foreach (String s1 in expressions)
            {
                String expr = s1.Trim();
                if (!String.IsNullOrEmpty(expr))
                {
                    String[] arr = expr.Split('{');
                    if (arr.Length != 2)
                        throw new Exception(String.Format("Css error: ", expr));
                    String selector = arr[0].Trim().ToLower();

                    String[] sel = selector.Split(' ');
                    selector = "";
                    foreach (String selPart in sel)
                    {
                        String ss = selPart.Trim();
                        if (!String.IsNullOrEmpty(ss))
                        {
                            if (tagNames.ContainsKey(ss))
                                ss = tagNames[ss].Name.ToLower();
                            if (!String.IsNullOrEmpty(selector))
                                selector += " ";
                            selector += ss;
                        }
                    }

                    foreach (String s2 in arr[1].Split(';'))
                    {
                        String style = s2.Trim();
                        if (!String.IsNullOrEmpty(style))
                        {
                            String[] arr2 = style.Split(':');
                            if (arr2.Length != 2)
                                throw new Exception(String.Format("Css error: ", style));
                            String attr = arr2[0].Trim();
                            String value = arr2[1].Trim();

                            if (!styleNames.ContainsKey(attr))
                                throw new Exception(String.Format("Style {0} is not found", attr));
                            Type attrType = styleNames[attr];

                            Style s = (Style)attrType.GetConstructor(new Type[] { }).Invoke(new object[] { });
                            s = s.FromString(value);

                            if (!Styles.ContainsKey(selector))
                                Styles.Add(selector, new List<Style>());
                            Styles[selector].Add(s);
                        }
                    }
                }
            }
        }

        static string ReadStream(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            List<char> chars = new List<char>((int)stream.Length);
            bool inCommentary = false;

            while (!reader.EndOfStream)
            {
                char c = (char)reader.Read();
                if (inCommentary)
                {
                    if (c == '*')
                    {
                        char nextChar = (char)reader.Read();
                        if (nextChar == '/')
                            inCommentary = false;
                    }
                }
                else if (c == '/')
                {
                    char nextChar = (char)reader.Read();
                    if (nextChar == '*')
                        inCommentary = true;
                    else
                    {
                        chars.Add(c);
                        chars.Add(nextChar);
                    }
                }
                else
                    chars.Add(c);
            }

            string expression = new string(chars.ToArray());
            return expression;
        }

        protected Dictionary<String, List<Style>> Styles { get; private set; }

        static bool IsStyleType(Type t)
        {
            return t.IsSubclassOf(typeof(BitMobile.Controls.StyleSheet.Style));
        }

        static void BuildNames()
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in a.GetTypes())
                {
                    // In namespace BitMoblie.Controls we can find classes for lambda and layout
                    if (!type.Name.Contains("DisplayClass")
                        && !type.Name.Contains("LayoutParams")
                        && !type.Name.Contains("Handler")
						&& !type.Name.Contains("AnonStorey"))
                    {
                        if (type.Namespace == null)
                            continue;

                        if (IsStyleType(type))
                        {
                            String s = type.Name.ToLower();
                            object[] attr = type.GetCustomAttributes(typeof(SynonymAttribute), false);
                            if (attr.Length == 1)
                                s = ((SynonymAttribute)attr[0]).Name;
                            styleNames.Add(s, type);
                        }
                        if (type.Namespace.Equals("BitMobile.Controls"))
                        {
                            String s = type.Name.ToLower();
                            object[] attr = type.GetCustomAttributes(typeof(SynonymAttribute), false);
                            if (attr.Length == 1)
                                s = ((SynonymAttribute)attr[0]).Name;
                            tagNames.Add(s, type);
                        }
                    }
                }

            }
        }
    }
}

