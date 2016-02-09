using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Debugger
{
    public class Debugger : BitMobile.Debugger.IDebugger, BitMobile.DbEngine.IDatabaseAware
    {
        private static Debugger debugger = null;

        public static Debugger CreateInstance(bool waitDebugger)
        {
            if (debugger == null)
                debugger = new Debugger(waitDebugger);
            return debugger;
        }

        private DebugInterpreter interpreter = null;
        private SqlManager sqlManager = null;

        private Debugger(bool waitDebugger)
        {
            DebugConsole.Init();
            sqlManager = SqlManager.CreateInstance();
            interpreter = DebugInterpreter.CreateInstance(waitDebugger);
        }

        public EventHandler<EventArgs> OnBreak
        {
            get
            {
                return interpreter.OnBreak;
            }
        }

        public int[] GetBreakPoints(String moduleName)
        {
            return interpreter.GetBreakPoints(moduleName);
        }

        public void WriteToConsole(String message)
        {
            DebugConsole.Console.WriteLine(message);
        }

        public void SetDatabase(BitMobile.DbEngine.IDatabase database)
        {
            sqlManager.SetDatabase(database);
        }
    }
}
