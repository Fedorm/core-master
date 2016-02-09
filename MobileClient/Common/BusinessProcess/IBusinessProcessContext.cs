using BitMobile.Common.BusinessProcess.Controllers;
using BitMobile.Common.BusinessProcess.Factory;
using BitMobile.Common.DbEngine;

namespace BitMobile.Common.BusinessProcess
{
    public interface IBusinessProcessContext
    {
        IGlobalEventsController GlobalEventsController { get; }
        IScreenController CreateScreenController(string controllerName);
        IBusinessProcessFactory CreateBusinessProcessFactory();
        IScreenFactory CreateScreenFactory();
        IConfigurationFactory CreateConfigurationFactory();
        IObjectFactory CreateObjectFactory();
        void InitConsole(IDatabase db = null);
    }
}
