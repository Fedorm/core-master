using System;
using System.IO;

namespace BitMobile.MVC
{
    public class HtmlView : BaseView
    {
        private ValueStack.ValueStack _stack;
        private readonly String _html;

        public HtmlView(ValueStack.ValueStack stack, String html)
        {
            _stack = stack;
            _html = html;
        }

        public override Stream Translate()
        {
            var output = new MemoryStream();
            var wr = new StreamWriter(output);
            wr.Write(_html);
            wr.Flush();
            output.Position = 0;
            return output;
        }

        public override String ContentType()
        {
            return "text/html";
        }
    }
}
