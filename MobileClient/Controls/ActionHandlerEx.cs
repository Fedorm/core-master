using System.Collections.Generic;
using BitMobile.Common.Controls;
using BitMobile.Common.ValueStack;

namespace BitMobile.Controls
{
    public class ActionHandlerEx : ActionHandlerAbstract, IActionHandlerEx
    {
        public ActionHandlerEx(string expression, IValueStack valueStack, object sender)
            : base(expression, valueStack, CreateParameters(sender))
        {
        }

        private static IList<object> CreateParameters(object sender)
        {
            return new List<object> { sender };
        }
    }
}