using System;


namespace BitMobile.ValueStack
{
    public static class ExpressionHelper
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

        public static bool IsEmpty(this DateTime value)
        {
            return value == null;
        }
    }
}