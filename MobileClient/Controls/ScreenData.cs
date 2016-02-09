using System;
using BitMobile.Common.Controls;

namespace BitMobile.Controls
{
    public class ScreenData : IScreenData
    {
        public string Name { get; private set; }
        public string ControllerName { get; private set; }
        public IScreen Screen { get; private set; }

        public ScreenData(string name, string controllerName, IScreen screen)
        {
            Name = name;
            ControllerName = controllerName;
            Screen = screen;
        }

        public void Dispose()
        {
            var disposable = Screen as IDisposable;
            if (disposable != null)
                disposable.Dispose();
            Screen = null;
        }
    }

}

