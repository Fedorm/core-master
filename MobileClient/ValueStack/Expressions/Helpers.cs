using System;

namespace BitMobile.ValueStack.Expressions
{
    public static class ExpressionHelper
    {
        public static bool Contains(this String s, String value)
        {
            if (s == null)
                return true;
            string[] values = value.ToLower().Split(' ');
            string text = s.ToLower();

            // ReSharper disable once LoopCanBeConvertedToQuery
            for (int i = 0; i < values.Length; i++)
                if (!text.Contains(values[i]))
                    return false;

            return true;
        }

        public static String TimeString(this DateTime value)
        {
            return value.ToShortTimeString();
        }

        public static String DateString(this DateTime value)
        {
            return value.ToShortDateString();
        }
    }
}