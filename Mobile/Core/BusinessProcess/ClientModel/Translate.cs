using BitMobile.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.ClientModel
{
    public class Translate
    {
        IApplicationContext _context;

        public Translate(IApplicationContext context)
        {
            _context = context;
        }

        public object this[string key]
        {
            get
            {
                return _context.DAL.TranslateString(key);
            }
        }
    }
}
