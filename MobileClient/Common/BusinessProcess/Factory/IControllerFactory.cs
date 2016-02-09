using System;
using BitMobile.Common.BusinessProcess.Controllers;

namespace BitMobile.Common.BusinessProcess.Factory
{
    public interface IControllerFactory
    {
        T CreateController<T>(String moduleName, bool isGlobal = false, bool assignGlobal = false) where T : class, IController, new();
        void ClearCache();
    }
}
