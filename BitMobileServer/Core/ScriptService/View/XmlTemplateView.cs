using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BitMobile.ValueStack;

namespace BitMobile.MVC
{
    public class XmlTemplateView : TemplateView
    {
        public XmlTemplateView(BitMobile.ValueStack.ValueStack stack, String rootFolder, String name)
            :base(stack, rootFolder, name)
        {
        }

        public override String ContentType()
        {
            return "text/xml";
        }
    }
}
