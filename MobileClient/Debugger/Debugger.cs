using System;
using BitMobile.Common.DbEngine;
using BitMobile.Common.Debugger;

namespace BitMobile.Debugger
{
    public class Debugger : IDebugger, IDatabaseAware
    {
        private static Debugger _debugger;

        public static Debugger CreateInstance()
        {
            return _debugger ?? (_debugger = new Debugger());
        }

        private readonly DebugInterpreter _interpreter;
        private readonly SqlManager _sqlManager;

        private Debugger()
        {
            DebugConsole.Init();
            _sqlManager = SqlManager.CreateInstance();
            _interpreter = DebugInterpreter.CreateInstance();
        }

        public EventHandler<EventArgs> OnBreak
        {
            get
            {
                return _interpreter.OnBreak;
            }
        }

        public int[] GetBreakPoints(String moduleName)
        {
            return _interpreter.GetBreakPoints(moduleName);
        }

        public void WriteToConsole(String message)
        {
            DebugConsole.Console.WriteLine(message);
        }
        
        public void SetDatabase(IDatabase database)
        {
            _sqlManager.SetDatabase(database);
        }
    }
}
