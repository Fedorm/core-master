using System;

namespace BitMobile.DbEngine
{
    public static partial class DbFunctions
    {
        public static string ToLower(string input)
        {
            return input.ToLower();
        }

        public static string ToUpper(string input)
        {
            return input.ToUpper();
        }

        public static bool Contains(string input, string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            if (!string.IsNullOrEmpty(input))
            {
                string[] values = value.ToLower().Split(' ');
                string s = input.ToLower();

                for (int i = 0; i < values.Length; i++)
                    if (!s.Contains(values[i]))
                        return false;
                return true;
            }
            else
                return false;
        }
    }
}

