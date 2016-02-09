using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    class Device : RemoteProxy
    {
        string _resourcePath;

        public Device(String address, Console console, string resourcePath)
            : base(address, console)
        {
            _resourcePath = resourcePath;
        }

        public string Click(string expression)
        {
            return DoRequestString("Click", expression);
        }

        public string SetText(string expression, string text)
        {
            return DoRequestString("SetText", expression, text);
        }

        public string SetFocus(string expression)
        {
            return DoRequestString("SetFocus", expression);
        }

        public string GetValue(string expression)
        {
            return DoRequestString("GetValue", expression);
        }

        public string SetValue(string expression, string property, object value)
        {
            return DoRequestString("SetValue", expression, property, value);
        }

        public string GetCount(string expression)
        {
            return DoRequestString("GetCount", expression);
        }

        public string ScrollTo(string expression, int index)
        {            
            return DoRequestString("ScrollTo", expression, index);
        }

        public void TakeScreenshot(string name)
        {
            using (Stream stream = DoRequestStream("TakeScreenshot", new object[0]))
            {
                if (!Directory.Exists(_resourcePath))
                    Directory.CreateDirectory(_resourcePath);

                string path = string.Format("{0}\\{1}_{2}_.jpg", _resourcePath, name, DateTime.Now.ToString("hh.mm.ss"));
                using (FileStream fileStream = new FileStream(path, FileMode.Create))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }
    }
}
