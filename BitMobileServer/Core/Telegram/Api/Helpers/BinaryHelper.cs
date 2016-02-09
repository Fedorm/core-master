using System;
using System.Collections.Generic;

namespace Telegram.Helpers
{
    public static class BinaryHelper
    {
        /// <summary>
        ///     Разделение коллекции на несколько одинаковых кусочков
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Section<T>(this IEnumerable<T> source, int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException("length");

            var section = new List<T>(length);

            foreach (T item in source)
            {
                section.Add(item);

                if (section.Count == length)
                {
                    yield return section.AsReadOnly();
                    section = new List<T>(length);
                }
            }

            if (section.Count > 0)
                yield return section.AsReadOnly();
        }

        public static byte[] StringToByteArray(String hex)
        {
            int numberChars = hex.Length;
            var bytes = new byte[numberChars/2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i/2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string ByteToHexBitFiddle(byte[] bytes)
        {
            var c = new char[bytes.Length*2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i] >> 4;
                c[i*2] = (char) (55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i*2 + 1] = (char) (55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }

        public static string ByteToHexBitFiddle(byte[] bytes, int length)
        {
            var c = new char[length*2];
            for (int i = 0; i < length; i++)
            {
                int b = bytes[i] >> 4;
                c[i*2] = (char) (55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i*2 + 1] = (char) (55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }
    }
}