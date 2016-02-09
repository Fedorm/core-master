using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitMobile.ValueStack
{
    public class Property : ValueStackTag
    {
        public Property()
        {
        }

        private String value;

        public String Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
    }
}
