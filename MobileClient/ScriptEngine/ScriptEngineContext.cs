using System;
using System.Collections.Generic;
using System.IO;
using BitMobile.Common.Debugger;
using BitMobile.Common.ScriptEngine;
using BitMobile.Script;

namespace BitMobile.ScriptEngine
{
    public class ScriptEngineContext : IScriptEngineContext
    {
        private readonly Dictionary<string, IScriptEngine> _scripts = new Dictionary<string, IScriptEngine>();
        
        public IScriptEngine LoadScript(Stream scriptStream, string name, IDebugger debugger)
        {
            if (_scripts.ContainsKey(name))
                return _scripts[name];

            var engine = new Script.ScriptEngine(name, debugger);
            _scripts.Add(name, engine);
            if (scriptStream != null)
                engine.Run(new StreamReader(scriptStream));
            return engine;
        }
        
        public void RegisterType(string name, Type type)
        {
            CachedTypeResolver.RegisterType(name, type);
        }
    }
}
