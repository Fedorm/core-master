using BitMobile.Script;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class Script
    {
        public static BitMobile.Script.ScriptEngine Engine { get; private set; }

        public static object RunTest(String address, String entryPoint, String fileName, string reportath, string resourcePath)
        {
            object result = null;

            if (File.Exists(fileName))
            {
                using (Stream scriptStream = File.OpenRead(fileName))
                {
                    Engine = BitMobile.Script.ScriptEngine.LoadScript(scriptStream, entryPoint, DateTime.Now);

                    Console console = new Console(reportath, Engine);
                    Engine.AddVariable("Console", console);
                    Engine.AddVariable("Device", new Device(address, console, resourcePath));
                    Engine.AddVariable("Stopwatch", new Stopwatch());
                    Engine.AddVariable("Dialog", new Dialog(address, console));

                    result = Engine.CallFunction(entryPoint);
                }
                return result;
            }
            else
                throw new Exception("Test file does not exist");
        }
    }
}
