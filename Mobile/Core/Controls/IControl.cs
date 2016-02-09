using BitMobile.Controls.StyleSheet;
using System;

namespace BitMobile.Controls
{
    public interface IControl<out T> : ILayoutable
    {
        T CreateView();

        T View { get; }

        object Parent { get; set; }
    }

    public interface ILayoutable : IStyledObject, IDisposable
    {
        Rectangle Frame { get; set; }
    }
}
