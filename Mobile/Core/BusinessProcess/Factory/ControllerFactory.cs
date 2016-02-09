using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using BitMobile.Controllers;
using BitMobile.ClientModel;
using BitMobile.Script;
using BitMobile.Application;
using BitMobile.ValueStack;
using BitMobile.Utilities.Develop;

namespace BitMobile.Factory
{
    public class ControllerFactory
    {
        private static bool globalModuledInitialized = false;
        private static ControllerFactory factory = null;
        private Dictionary<String, Controller> controllers;
        private Dictionary<String, GlobalModuleController> globalControllers;
        private GlobalEventsController globalEventsController = null;

        private static BitMobile.Debugger.IDebugger debugger;

        public static BitMobile.Debugger.IDebugger Debugger
        {
            get { return ControllerFactory.debugger; }
            set { ControllerFactory.debugger = value; }
        }

        public static ControllerFactory CreateInstance()
        {
            if (factory == null)
            {
                InitializeScriptEngine();
                factory = new ControllerFactory();
                DoWarmupActions(factory);
                factory.InitGlobalControllers();
                factory.GlobalEventsController().OnApplicationInit();
            }
            return factory;
        }

        public static GlobalEventsController GlobalEvents
        {
            get
            {
                return CreateInstance().globalEventsController;
            }
        }

        private ControllerFactory()
        {
            controllers = new Dictionary<string, Controller>();
            globalControllers = new Dictionary<string, GlobalModuleController>();
        }


        private void InitGlobalControllers()
        {
            if (!globalModuledInitialized)
            {
                foreach (BitMobile.Configuration.Module m in ApplicationContext.Context.Configuration.Script.GlobalModules.Controls)
                {
                    globalControllers.Add(m.Name, CreateController<GlobalModuleController>(m.File, true, true));
                }
                globalModuledInitialized = true;
            }
        }

        private void AssignGlobalControllers(ScriptEngine engine)
        {
            foreach (KeyValuePair<String, GlobalModuleController> pair in globalControllers)
            {
                engine.AddVariable(pair.Key, pair.Value);
            }
        }

        public GlobalEventsController GlobalEventsController()
        {
            if (globalEventsController != null)
                return globalEventsController;
            else
                globalEventsController = CreateController<GlobalEventsController>(ApplicationContext.Context.Configuration.Script.GlobalEvents.File, true, true); //TODO check if null !!!
            return globalEventsController;
        }

        public T CreateController<T>(String moduleName, bool isGlobal = false, bool assignGlobal = false) where T : Controller, new()
        {
            TimeStamp.Start("CreateController");

            Controller controller = null;
            if (!controllers.TryGetValue(moduleName, out controller))
            {
                Stream scriptStream = null;
                ApplicationContext.Context.DAL.TryGetScriptByName(moduleName, out scriptStream);
                if (scriptStream != null && !isGlobal)
                    scriptStream = ApplyMixins(moduleName, scriptStream);

                //scriptStream CAN be null !!!
                ScriptEngine scriptEngine = ScriptEngine.LoadScript(scriptStream, moduleName, Debugger);

                //init script variables ant types here..
                scriptEngine.AddVariable("String", typeof(String));
                scriptEngine.AddVariable("DateTime", typeof(DateTime));
                scriptEngine.AddVariable("Workflow", new Workflow());
                scriptEngine.AddVariable("DB", new DB(scriptEngine, ApplicationContext.Context));
                scriptEngine.AddVariable("GPS", new GPS(ApplicationContext.Context.LocationProvider));
                scriptEngine.AddVariable("GPSTracking", new GPSTracking(ApplicationContext.Context.LocationTracker));
				scriptEngine.AddVariable ("Gallery", new Gallery (ApplicationContext.Context, scriptEngine));

                Variables v = new Variables(ApplicationContext.Context);
                scriptEngine.AddVariable("Variables", v);
                scriptEngine.AddVariable("$", v);

                scriptEngine.AddVariable("Dialog", new Dialog(scriptEngine, ApplicationContext.Context));
                scriptEngine.AddVariable("Translate", new Translate(ApplicationContext.Context));
                scriptEngine.AddVariable("Converter", new Converter());
                scriptEngine.AddVariable("Phone", new Phone(ApplicationContext.Context));
                scriptEngine.AddVariable("FileSystem", new FileSystem(scriptEngine, ApplicationContext.Context));
                scriptEngine.AddVariable("Camera", new Camera(scriptEngine, ApplicationContext.Context));
                scriptEngine.AddVariable("Console", new BitMobile.ClientModel.Console(scriptEngine, ApplicationContext.Context));
                scriptEngine.AddVariable("BarcodeScanner", new BarcodeScanner(scriptEngine, ApplicationContext.Context));
                scriptEngine.AddVariable("Application", new ClientModel.Application(ApplicationContext.Context));
                scriptEngine.AddVariable("Clipboard", new Clipboard(ApplicationContext.Context));
                
                if (!isGlobal || assignGlobal)
                    AssignGlobalControllers(scriptEngine);

                controller = new T();
                controller.Init(scriptEngine);//, isGlobal ? null : GlobalEventsController());
                controllers.Add(moduleName, controller);
            }

            AssignBreakPoints(controller, true);

            TimeStamp.Log("CreateController", moduleName);

            return (T)controller;
        }

        private void AssignBreakPoints(Controller controller, bool applyGlobal = false)
        {
            if (Debugger != null)
            {
                if (applyGlobal)
                {
                    foreach (Controller c in globalControllers.Values)
                        AssignBreakPoints(c, false);
                }

                //scriptEngine.Step += (sender, args) => Debugger.OnBreak(sender, args);
                controller.ScriptEngine.Break -= OnBreak;
                int[] breakPoints = Debugger.GetBreakPoints(controller.ScriptEngine.ModuleName);
                if (breakPoints != null)
                {
                    if (breakPoints.Length > 0)
                    {
                        controller.ScriptEngine.BreakPoints.Clear();
                        foreach (int line in breakPoints)
                        {
                            Jint.Debugger.BreakPoint bp = new Jint.Debugger.BreakPoint(line, 0);
                            controller.ScriptEngine.BreakPoints.Add(bp);
                        }
                        controller.ScriptEngine.Break += OnBreak;
                    }
                }
            }
        }

        private void OnBreak(object sender, Jint.Debugger.DebugInformation e)
        {
            Debugger.OnBreak(sender, e);
        }

        private System.IO.Stream ApplyMixins(String moduleName, System.IO.Stream iStream)
        {
            MemoryStream result = null;
            foreach (BitMobile.Configuration.Mixin m in ApplicationContext.Context.Configuration.Script.Mixins.Controls)
            {
                System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex(GetPattern(m.Target));
                if (re.IsMatch(moduleName))
                {
                    System.IO.Stream ms = null;
                    if (ApplicationContext.Context.DAL.TryGetScriptByName(m.File, out ms))
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
            return result == null ? iStream : result;
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
            ScriptEngine.RegisterType("DateTime", typeof(DateTime));
            ScriptEngine.RegisterType("Guid", typeof(Guid));
            ScriptEngine.RegisterType("Query", typeof(Query));
            ScriptEngine.RegisterType("Workflow", typeof(Workflow));
            ScriptEngine.RegisterType("DialogResult", typeof(Dialog.Result));
            ScriptEngine.RegisterType("Dictionary", typeof(CustomDictionary));
            ScriptEngine.RegisterType("List", typeof(System.Collections.ArrayList));
            ScriptEngine.RegisterType("HttpRequest", typeof(BitMobile.ClientModel.HttpRequest));
        }

        private static void DoWarmupActions(ControllerFactory f)
        {
            foreach (Configuration.WarmupAction wa in ApplicationContext.Context.Configuration.Script.WarmupActions.Controls)
            {
                ScreenController sc = f.CreateController<ScreenController>(wa.Controller);
                sc.CallFunctionNoException(wa.Function, new object[] { });
            }
        }
    }
}
