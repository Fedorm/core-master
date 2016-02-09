using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Configuration
{
    public class GlobalEvents
    {
        private String file;
        public String File
        {
            get
            {
                return file;
            }
            set
            {
                file = value;
            }
        }

        public GlobalEvents()
        {
        }
    }
}

