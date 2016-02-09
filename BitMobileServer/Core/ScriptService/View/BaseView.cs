using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitMobile.MVC
{
    public abstract class BaseView
    {
        public abstract System.IO.Stream Translate();
        public abstract String ContentType();
    }
}
