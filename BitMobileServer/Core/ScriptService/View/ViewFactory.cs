using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitMobile.MVC
{
    public class ViewFactory
    {
        private String rootFolder;
        private ValueStack.ValueStack stack;

        public ViewFactory(String rootFolder, ValueStack.ValueStack stack)
        {
            this.rootFolder = rootFolder;
            this.stack = stack;
        }

        public TemplateView TemplateView(String name)
        {
            return new TemplateView(stack, rootFolder, name);
        }

        public XmlTemplateView XmlTemplateView(String name)
        {
            return new XmlTemplateView(stack, rootFolder, name);
        }

        public HtmlView HtmlView(String html)
        {
            return new HtmlView(stack, html);
        }

    }
}
