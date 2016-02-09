using System;
using BitMobile.Common.Application;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.Common.ValueStack;

namespace BitMobile.Controls
{
    public class ControlsContext : IControlsContext
    {
        public ControlsContext()
        {
            ActionHandlerLocker = new ActionHandlerLocker();
        }

        public ILocker ActionHandlerLocker { get; private set; }

        public IDataBinder CreateDataBinder(object control, string controlPropertyName, object obj, string objPropertyName)
        {
            return new DataBinder(control, controlPropertyName, obj, objPropertyName);
        }

        public IActionHandler CreateActionHandler(string expression, IValueStack valueStack)
        {
            return new ActionHandler(expression, valueStack);
        }

        public IActionHandlerEx CreateActionHandlerEx(string expression, IValueStack valueStack, object sender)
        {
            return new ActionHandlerEx(expression, valueStack, sender);
        }

        public IScreenData CreateScreenData(string name, string controllerName, IScreen screen)
        {
            return new ScreenData(name, controllerName, screen);
        }

        public ISwipeBehaviour CreateSwipeBehaviour(Action<float> scroll)
        {
            return new SwipeBehaviour(scroll);
        }

        public ILayoutBehaviour CreateLayoutBehaviour(IStyleSheet styleSheet, ILayoutable container)
        {
            return new LayoutBehaviour(styleSheet, container);
        }

        public IRectangle CreateRectangle(float left, float top, float width, float height)
        {
            return new Rectangle(left, top, width, height);
        }

        public IRectangle CreateRectangle(float left, float top, IBound bound)
        {
            return new Rectangle(left, top, bound);
        }

        public IWebMapBehavior CreateWebMapBehavior()
        {
            return new GoogleMapBehavior();
        }

        public IInputValidator CreateInputValidator(object obj, string text)
        {
            return new InputValidator(obj, text);
        }

        public ILayoutableContainerBehaviour<T> CreateContainerBehaviour<T>(ILayoutableContainer container) 
            where T : class, ILayoutable
        {
            return new LayoutableContainerBehaviour<T>(container);
        }
    }
}
