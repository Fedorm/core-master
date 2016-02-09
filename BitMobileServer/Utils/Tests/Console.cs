using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BitMobile.Script;

namespace Tests
{
    public class Console
    {
        string _reportPath;
        private BitMobile.Script.ScriptEngine _engine;

        public Console(string reportPath, BitMobile.Script.ScriptEngine engine)
        {
            _reportPath = reportPath;
            _engine = engine;

            CommandPause = 1000;
        }

        public int CommandPause { get; set; }

        public string CurrentLine
        {
            get
            {
                return Script.Engine.CurrentLine;
            }
        }

        public void Pause(int milliseconds)
        {
            System.Threading.Thread.Sleep(milliseconds);
        }

        public void WriteLine(object value)
        {
            WriteLine(value, false);
        }

        public void WriteLine(object value, bool showLineNumber)
        {
            string msg;
            if (value != null)
                msg = value.ToString();
            else
                msg = "(null)";

            if (showLineNumber)
                msg = string.Format("{0}: {1}", Script.Engine.CurrentLine, msg);

            System.Console.WriteLine(msg);
            try
            {
                using (StreamWriter writer = new StreamWriter(_reportPath, true))
                    writer.WriteLine(msg);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }

        public void Terminate(bool condition, string message)
        {
            if (condition)
            {
                WriteLine(message);
                WriteLine("Test has been terminated");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }
    }
}
