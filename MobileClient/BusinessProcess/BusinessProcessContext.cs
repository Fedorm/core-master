using System;
using BitMobile.Application.Debugger;
using BitMobile.BusinessProcess.Controllers;
using BitMobile.BusinessProcess.Factory;
using BitMobile.Common.BusinessProcess;
using BitMobile.Common.BusinessProcess.Controllers;
using BitMobile.Common.BusinessProcess.Factory;
using BitMobile.Common.DbEngine;
using BitMobile.Common.Develop;

namespace BitMobile.BusinessProcess
{
    public class BusinessProcessContext : IBusinessProcessContext
    {
        public IGlobalEventsController GlobalEventsController
        {
            get { return ControllerFactory.GlobalEvents; }
        }

        public IScreenController CreateScreenController(string controllerName)
        {
            return ControllerFactory.CreateInstance().CreateController<ScreenController>(controllerName);
        }

        public IBusinessProcessFactory CreateBusinessProcessFactory()
        {
            return BusinessProcessFactory.CreateInstance();
        }

        public IScreenFactory CreateScreenFactory()
        {
            return ScreenFactory.CreateInstance();
        }

        public IConfigurationFactory CreateConfigurationFactory()
        {
            return ConfigurationFactory.CreateInstance();
        }

        public IObjectFactory CreateObjectFactory()
        {
            return new ObjectFactory();
        }

        public void InitConsole(IDatabase db = null)
        {
            if (ControllerFactory.Debugger == null)
            {
                ControllerFactory.Debugger = DebugContext.Current.CreateDebugger();
                TimeStamp.Write += ControllerFactory.Debugger.WriteToConsole;
                TimeCollector.Write += ControllerFactory.Debugger.WriteToConsole;
            }
            else
                // ReSharper disable once PossibleNullReferenceException
                (ControllerFactory.Debugger as IDatabaseAware).SetDatabase(db);
        }
    }
}
