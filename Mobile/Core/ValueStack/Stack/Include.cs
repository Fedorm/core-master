using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.ValueStack
{
    public class Include : ValueStackTag
    {
        public Include()
        {
        }

        private String file;

        public String File
        {
            get { return this.file; }
            set { this.file = value; }
        }

    }
}