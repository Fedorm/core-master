using BitMobile.Common.ScriptEngine;
using BitMobile.Common.ValueStack;
using BitMobile.ValueStack;

namespace BitMobile.Common.BusinessProcess.Controllers
{
    public interface IController : IExternalFunction
    {
        IScriptEngine ScriptEngine { get; }
        void Init(IScriptEngine scriptEngine);
    }
}
