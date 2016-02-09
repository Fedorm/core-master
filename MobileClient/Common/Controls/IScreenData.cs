using System;

namespace BitMobile.Common.Controls
{
    public interface IScreenData: IDisposable
    {
        String Name { get; }
        String ControllerName { get; }
        IScreen Screen { get; }
    }
}
