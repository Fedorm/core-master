using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BitMobile.Script;
using BitMobile.Application;

namespace BitMobile.ClientModel
{
    public class Console
    {
        ScriptEngine _scriptEngine;
        IApplicationContext _context;

        public Console(ScriptEngine scriptEngine, IApplicationContext context)
        {
            _scriptEngine = scriptEngine;
            _context = context;
        }

        public void WriteLine(String s)
        {
            if (_scriptEngine.Debugger != null)
                _scriptEngine.Debugger.WriteToConsole(s);
        }
    }
}