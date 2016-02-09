using System;
using BitMobile.Common.Application;
using BitMobile.Common.StyleSheet;
using BitMobile.Common.ValueStack;

namespace BitMobile.Common.Controls
{
    public interface IControlsContext
    {
        ILocker ActionHandlerLocker { get; }
        IDataBinder CreateDataBinder(object control, string controlPropertyName, object obj, string objPropertyName);
        IActionHandler CreateActionHandler(string expression, IValueStack valueStack);
        IActionHandlerEx CreateActionHandlerEx(string expression, IValueStack valueStack, object sender);
        IScreenData CreateScreenData(string name, string controllerName, IScreen screen);
        ISwipeBehaviour CreateSwipeBehaviour(Action<float> scroll);
        ILayoutBehaviour CreateLayoutBehaviour(IStyleSheet styleSheet, ILayoutable container);
        IRectangle CreateRectangle(float left, float top, float width, float height);
        IRectangle CreateRectangle(float left, float top, IBound bound);
        IWebMapBehavior CreateWebMapBehavior();
        IInputValidator CreateInputValidator(object obj, string text);
        ILayoutableContainerBehaviour<T> CreateContainerBehaviour<T>(ILayoutableContainer container)
            where T : class, ILayoutable;
    }
}
