using System;
using System.Diagnostics;

namespace BitMobile.Common.Develop
{
    public static class Assert
    {
        [Conditional("DEBUG")]
        public static void True(bool condition, string message = null)
        {
            if (!condition)
                Fail(message);
        }

        [Conditional("DEBUG")]
        public static void AreEqual(object expected, object actual, string message = null)
        {
            if (expected != actual)
                Fail(message);
        }

        [Conditional("DEBUG")]
        public static void AreEqual<T>(T expected, T actual, string message = null)
        {
            if (typeof(T).IsValueType)
            {
                if (!expected.Equals(actual))
                    Fail(message);
            }
            else if (!Equals(expected, actual))
                Fail(message);
        }

        [Conditional("DEBUG")]
        public static void IsNotNull(object value, string message = null)
        {
            if (value == null)
                Fail(message);
        }

        [Conditional("DEBUG")]
        public static void IsNull(object value, string message = null)
        {
            if (value != null)
                Fail(message);
        }

        [Conditional("DEBUG")]
        private static void Fail(string message = null)
        {
            try
            {
                throw new Exception();
            }
            catch
            {
                Debug.Assert(false, message);
            }
        }
    }
}