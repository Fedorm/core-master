using System;
using System.Collections;
using System.Linq;

namespace BitMobile.ExpressionEvaluator
{
    static class Helper
    {
        public static bool Contains(this String s, String value)
        {
            if (s == null)
                return true;
            else
            {
                string[] values = value.ToLower().Split(' ');
                string text = s.ToLower();

                for (int i = 0; i < values.Length; i++)
                    if (!text.Contains(values[i]))
                        return false;

                return true;
            }
        }

        public static String TimeString(this DateTime value)
        {
            return value.ToShortTimeString();
        }

        public static String DateString(this DateTime value)
        {
            return value.ToShortDateString();
        }

        public static bool IsEmpty(this object value)
        {
            if (value == null)
                return true;

            object dafaultValue = Activator.CreateInstance(value.GetType());
            return value.Equals(dafaultValue);
        }

        public static bool In(this object value, IEnumerable collection)
        {
            foreach (var item in collection)
                if (value.Equals(item))
                    return true;
            return false;
        }
    }
}