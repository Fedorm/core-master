using System.Collections.Generic;
using BitMobile.Common.Controls;
using BitMobile.Common.ValueStack;

namespace BitMobile.Controls
{
    public class ActionHandler : ActionHandlerAbstract, IActionHandler
    {
        public ActionHandler(string expression, IValueStack valueStack)
            : base(expression, valueStack, new List<object>())
        {
        }
    }
}