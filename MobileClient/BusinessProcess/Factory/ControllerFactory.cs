using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BitMobile.Application;
using BitMobile.Application.ScriptEngine;
using BitMobile.Application.ValueStack;
using BitMobile.BusinessProcess.ClientModel;
using BitMobile.BusinessProcess.Controllers;
using BitMobile.BusinessProcess.SolutionConfiguration;
using BitMobile.Common.BusinessProcess.Factory;
using BitMobile.Common.Debugger;
using BitMobile.Common.Develop;
using BitMobile.Common.ScriptEngine;
using Console = BitMobile.BusinessProcess.ClientModel.Console;
using BitMobile.Common.BusinessProcess.Controllers;

namespace BitMobile.BusinessProcess.Factory
{
    public class ControllerFactory : IControllerFactory
    {
        private static bool _globalModuledInitialized;
        private static ControllerFactory _factory;
        private readonly Dictionary<String, IController> _controllers;
        private readonly Dictionary<String, GlobalModuleController> _globalControllers;
        private GlobalEventsController _globalEventsController;

        public static IDebugger Debugger { get; set; }

        public static ControllerFactory CreateInstance()
        {
            if (_factory == null)
            {
                InitializeScriptEngine();
                _factory = new ControllerFactory();
                _factory.InitGlobalControllers();
                _factory.GlobalEventsController();
                _factory.InitControllers();
                DoWarmupActions(_factory);
                _factory.GlobalEventsController().OnApplicationInit();
            }
            return _factory;
        }

        private ControllerFactory()
        {
            _controllers = new Dictionary<string, IController>();
            _globalControllers = new Dictionary<string, GlobalModuleController>();
        }

        public static GlobalEventsController GlobalEvents
        {
            get
            {
                return CreateInstance()._globalEventsController;
            }
        }

        public T CreateController<T>(string moduleName, bool isGlobal = false, bool assignGlobal = false) where T : class, IController, new()
        {
            if (!moduleName.Trim().EndsWith(".js"))
                return null;
            
            moduleName = ValidateModuleName(moduleName);

            IController controller;
            if (!_controllers.TryGetValue(moduleName, out controller))
            {
                Stream scriptStream;
                ApplicationContext.Current.Dal.TryGetScriptByName(moduleName, out scriptStream);
                if (scriptStream != null && !isGlobal)
                    scriptStream = ApplyMixins(moduleName, scriptStream);

                //scriptStream CAN be null !!!
                IScriptEngine scriptEngine = ScriptEngineContext.Current.LoadScript(scriptStream, moduleName, Debugger);

                //init script variables ant types here..
                scriptEngine.AddVariable("String", typeof(String));
                scriptEngine.AddVariable("DateTime", typeof(DateTime));
                scriptEngine.AddVariable("Workflow", new Workflow());
                scriptEngine.AddVariable("DB", new Db(scriptEngine, ApplicationContext.Current));
                scriptEngine.AddVariable("GPS", new Gps(ApplicationContext.Current.LocationProvider));
                scriptEngine.AddVariable("GPSTracking", new GpsTracking(ApplicationContext.Current.LocationTracker));
                scriptEngine.AddVariable("Gallery", new Gallery(ApplicationContext.Current, scriptEngine));

                var v = new Variables(ApplicationContext.Current);
                scriptEngine.AddVariable("Variables", v);
                scriptEngine.AddVariable("$", v);
                
                scriptEngine.AddVariable("Dialog", new Dialog(scriptEngine, ApplicationContext.Current));
                scriptEngine.AddVariable("Translate", new Translate(ApplicationContext.Current));
                scriptEngine.AddVariable("Converter", new Converter());
                scriptEngine.AddVariable("Phone", new Phone(ApplicationContext.Current));
                scriptEngine.AddVariable("FileSystem", new FileSystem(scriptEngine, ApplicationContext.Current));
                scriptEngine.AddVariable("Camera", new Camera(scriptEngine, ApplicationContext.Current));
                scriptEngine.AddVariable("Console", new Console(scriptEngine));
                scriptEngine.AddVariable("BarcodeScanner", new BarcodeScanner(scriptEngine, ApplicationContext.Current));
                scriptEngine.AddVariable("Application", new ClientModel.Application(scriptEngine, ApplicationContext.Current));
                scriptEngine.AddVariable("Clipboard", new Clipboard(ApplicationContext.Current));
                scriptEngine.AddVariable("Email", new Email(scriptEngine, ApplicationContext.Current));
                scriptEngine.AddVariable("PushNotification", new PushNotification(scriptEngine, ApplicationContext.Current));
                scriptEngine.AddVariable("LocalNotification", new LocalNotification(ApplicationContext.Current));
                scriptEngine.AddVariable("CurrentController", new CurrentController());
                scriptEngine.AddVariable("Web", new Web(scriptEngine, ApplicationContext.Current.WebProvider));

                if (!isGlobal || assignGlobal)
                    AssignGlobalControllers(scriptEngine);

                controller = new T();
                controller.Init(scriptEngine);//, isGlobal ? null : GlobalEventsController());
                _controllers.Add(moduleName, controller);
            }

            AssignBreakPoints(controller, true);
            
            return controller as T;
        }

        private static string ValidateModuleName(string name)
        {
            name = name.Replace('/', '\\').Replace("\\\\", "\\");
            if (name.StartsWith("\\"))
                name = name.Substring(1);
            return name;
        }

        private void InitGlobalControllers()
        {
            if (!_globalModuledInitialized)
            {
                foreach (Module m in GetGlobalModules())
                {
                    _globalControllers.Add(m.Name, CreateController<GlobalModuleController>(m.File, true, true));
                }
                _globalModuledInitialized = true;
            }
        }

        private void InitControllers()
        {
            string[] controllers = ApplicationContext.Current.Dal.GetResources("Script");
            foreach (string controller in controllers)
                CreateController<ScreenController>(controller);
        }

        public void ClearCache()
        {
            List<String> values = new List<string>();
            foreach (var entry in _controllers)
            {
                if (entry.Value is ScreenController)
                    values.Add(entry.Key);
            }
            foreach (String key in values)
                _controllers.Remove(key);
        }

        private object[] GetGlobalModules()
        {
            return ApplicationContext.Current.Configuration.Script.GlobalModules.Controls;
        }

        private void AssignGlobalControllers(IScriptEngine engine)
        {
            foreach (KeyValuePair<String, GlobalModuleController> pair in _globalControllers)
            {
                engine.AddVariable(pair.Key, pair.Value);
            }
        }

        private GlobalEventsController GlobalEventsController()
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_globalEventsController == null)
                _globalEventsController = CreateController<GlobalEventsController>(ApplicationContext.Current.Configuration.Script.GlobalEvents.File, true, true); //TODO check if null !!!
            return _globalEventsController;
        }

        private void AssignBreakPoints(IController controller, bool applyGlobal = false)
        {
            if (Debugger != null)
            {
                if (applyGlobal)
                    foreach (GlobalModuleController c in _globalControllers.Values)
                        AssignBreakPoints(c);

                controller.ScriptEngine.ApplyBreakPoints();
            }
        }

        private Stream ApplyMixins(String moduleName, Stream iStream)
        {
            MemoryStream result = null;
            foreach (Mixin m in ApplicationContext.Current.Configuration.Script.Mixins.Controls)
            {
                var re = new System.Text.RegularExpressions.Regex(GetPattern(m.Target));
                if (re.IsMatch(moduleName))
                {
                    Stream ms;
                    if (ApplicationContext.Current.Dal.TryGetScriptByName(m.File, out ms))
                    {
                        if (result == null)
                        {
                            result = new MemoryStream();
                            iStream.CopyTo(result);
                        }
                        result.Seek(0, SeekOrigin.End);
                        ms.CopyTo(result);
                        result.Seek(0, SeekOrigin.Begin);
                    }
                    else
                        throw new Exception(String.Format("Cant apply mixin. {0} not found", m.File));
                }
            }
            return result ?? iStream;
        }

        private String GetPattern(String mask)
        {
            mask = mask.Replace(@"\", @"\\");
            mask = mask.Replace(".", @"\.");
            mask = mask.Replace("*", ".+");
            mask = mask.Replace("_", ".");
            return mask;
        }

        private static void InitializeScriptEngine()
        {
            IScriptEngineContext context = ScriptEngineContext.Current;
            context.RegisterType("DateTime", typeof(DateTime));
            context.RegisterType("Guid", typeof(Guid));
            context.RegisterType("Query", typeof(Query));
            context.RegisterType("Workflow", typeof(Workflow));
            context.RegisterType("DialogResult", typeof(Dialog.Result));
            context.RegisterType("Dictionary", ValueStackContext.Current.CreateDictionary().GetType());
            context.RegisterType("List", typeof(System.Collections.ArrayList));
            context.RegisterType("HttpRequest", typeof(HttpRequest));
        }

        private static void DoWarmupActions(ControllerFactory f)
        {
            foreach (WarmupAction wa in ApplicationContext.Current.Configuration.Script.WarmupActions.Controls)
            {
                var sc = f.CreateController<ScreenController>(wa.Controller);
                sc.CallFunctionNoException(wa.Function, new object[] { });
            }
        }
    }
}
