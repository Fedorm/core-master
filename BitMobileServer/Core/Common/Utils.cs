using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Utils
    {
        public static string MakeDetailedExceptionString(Exception e)
        {
            String text = e.Message;
            while (e.InnerException != null)
            {
                text = text + "; " + e.InnerException.Message;
                e = e.InnerException;
            }
            text = text + e.StackTrace;
            return text;
        }

        public static System.IO.Stream MakeTextAnswer(string text, params object[] args)
        {
            return MakeTextAnswer(string.Format(text, args));
        }

        public static System.IO.Stream MakeTextAnswer(String text)
        {
            MemoryStream ms = new MemoryStream();
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
            ms.Write(bytes, 0, bytes.Length);
            ms.Position = 0;
            return ms;
        }

        public static System.IO.Stream MakeExceptionAnswer(Exception e)
        {
            String text = e.Message;
            while (e.InnerException != null)
            {
                text = text + "; " + e.InnerException.Message;
                e = e.InnerException;
            }
            text = text + e.StackTrace;
            return MakeTextAnswer(text);
        }

        public static System.IO.Stream MakeExceptionAnswer(Exception e, String scope)
        {
            try
            {
                String text = e.Message;
                while (e.InnerException != null)
                {
                    text = text + "; " + e.InnerException.Message;
                    e = e.InnerException;
                }
                text = text + e.StackTrace;

                Common.Solution.Log(scope, "system", text);

                return MakeTextAnswer(text);
            }
            catch (Exception e2)
            {
                return MakeTextAnswer(e2.Message);
            }
        }

        public static string MakeExceptionString(Exception e)
        {
            String text = e.Message;
            while (e.InnerException != null)
            {
                text = text + "; " + e.InnerException.Message;
                e = e.InnerException;
            }
            return text;
        }

        public static string MakeExceptionString(Exception e, string ExceptionMethodName)
        {
            String text = string.Format("Throwed exception in the {0} method. Message:{1}", ExceptionMethodName, e.Message);
            while (e.InnerException != null)
            {
                text = text + "; " + e.InnerException.Message;
                e = e.InnerException;
            }
            return text;
        }

    }
}
