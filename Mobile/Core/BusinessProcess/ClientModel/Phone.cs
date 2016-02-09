using BitMobile.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.ClientModel
{
    class Phone
    {
        IApplicationContext _context;

        public Phone(IApplicationContext context)
        {
            _context = context;
        }

        public void Call(string number)
        {
            _context.PhoneCall(number);
        }
    }
}
