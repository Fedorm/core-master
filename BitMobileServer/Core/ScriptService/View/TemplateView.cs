using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BitMobile.ValueStack;

namespace BitMobile.MVC
{
    public class TemplateView : BaseView
    {
        private BitMobile.ValueStack.ValueStack stack;
        private String name;
        private String rootFolder;

        public TemplateView(BitMobile.ValueStack.ValueStack stack, String rootFolder, String name)
        {
            this.stack = stack;
            this.rootFolder = rootFolder;
            this.name = name;
        }

        public override System.IO.Stream Translate()
        {
            String fName = String.Format(@"{0}\{1}", rootFolder, name);
            Translator t = new Translator(rootFolder);
            System.IO.MemoryStream output = new System.IO.MemoryStream();
            using(System.IO.Stream input = System.IO.File.OpenRead(fName))
            {
                t.Translate(stack, input, output);
            }
            output.Position = 0;
            return output;
        }

        public override String ContentType()
        {
            return "text/html";
        }
    }
}
