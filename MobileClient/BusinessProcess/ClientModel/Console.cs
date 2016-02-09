using BitMobile.Application;
using BitMobile.Common.ScriptEngine;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    public class Console
    {
        private readonly IScriptEngine _scriptEngine;

        public Console(IScriptEngine scriptEngine)
        {
            _scriptEngine = scriptEngine;
        }

        public void WriteLine(object input)
        {
            if (ApplicationContext.Current.Settings.DevelopModeEnabled)
            {
                string s = input != null ? input.ToString() : "null";
                if (_scriptEngine.Debugger != null)
                    _scriptEngine.Debugger.WriteToConsole(s);

                System.Console.WriteLine("-> {0}", s);
            }
        }
    }
}