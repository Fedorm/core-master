using System;
using System.Collections;
using System.Reflection;

namespace BitMobile.Utilities.LogManager
{
    public static class LogSerializer
    {
        const int MAX_DEPTH = 10;

        static string Offset(int count)
        {
            return new string(' ', count);
        }

        public static string ObjToString(object obj, int offset, int depth)
        {
            string result = "";

            depth++;
            offset++;

            if (obj != null)
            {
                try
                {
                    string objToString = obj.ToString();
                    objToString = objToString == null ? "null" : Correct(objToString);

                    string typeToString = Correct(obj.GetType().ToString());

                    result += Offset(offset) + "<" + typeToString + ">";

                    if (objToString == typeToString)
                    {
                        if (depth <= MAX_DEPTH)
                        {
                            IEnumerable enumerable = obj as IEnumerable;
                            if (enumerable != null)
                            {
                                offset++;
                                result += Environment.NewLine;
                                result += Offset(offset) + "<Items>";
                                int i = 0;
                                result += Environment.NewLine;
                                foreach (var item in enumerable)
                                {
                                    result += ObjToString(item, offset, depth);
                                    i++;
                                    if (i > 50)
                                    {
                                        result += Offset(offset) + "<Break/>";
                                        break;
                                    }
                                }
                                result += Offset(offset) + "</Items>";
                                offset--;
                            }
                            else
                            {
                                offset++;
                                result += Environment.NewLine;
                                result += Offset(offset) + "<Properties>";
                                offset++;
                                foreach (var pi in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty))
                                    if (pi.GetCustomAttribute(typeof(NonLogAttribute)) == null)
                                    {
                                        var val = pi.GetValue(obj);

                                        string name = Correct(pi.Name);
                                        result += Environment.NewLine;
                                        result += Offset(offset) + "<" + name + ">";
                                        result += Environment.NewLine;
                                        result += ObjToString(val, offset, depth);
                                        result += Offset(offset) + "</" + name + ">";
                                    }
                                offset--;
                                result += Environment.NewLine;
                                result += Offset(offset) + "</Properties>";
                                offset--;
                            }
                        }
                        else
                            result += Offset(offset) + "ERROR: Max depth achieved!";

                        result += Environment.NewLine + Offset(offset);
                    }
                    else
                    {
                        result += obj.ToString();
                    }

                    result += "</" + typeToString + ">";

                }
                catch (Exception e)
                {
                    result = Environment.NewLine;
                    result += "<![CDATA[";
                    result += e.ToString();
                    result += "]]>";
                }
            }
            else
                result += Offset(offset) + "NULL";

            result += Environment.NewLine;

            return result;
        }

        static string Correct(string input)
        {
            return input.Replace('`', '_')
                .Replace('{', '_')
                .Replace('}', '_')
                .Replace('[', '_')
                .Replace(']', '_')
				.Replace('<', '_')
				.Replace('>', '_')
                .Replace('\'', '_')
                .Replace(',', '_')
                .Replace(';', '_')
                .Replace(':', '_')
                .Replace(' ', '_');
        }
    }
}