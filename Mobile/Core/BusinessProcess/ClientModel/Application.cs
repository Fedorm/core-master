using System;
using System.Collections.Generic;
using System.Text;
using BitMobile.Application;

namespace BitMobile.ClientModel
{
    public class Application
    {
        private readonly IApplicationContext _context;

        public Application(IApplicationContext context)
        {
            _context = context;
        }

        public void Exit()
        {
            _context.Exit(false);
        }

        public void Logout()
        {
            _context.Exit(true);
        }
    }
}
