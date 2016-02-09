using System;
using System.Collections.Generic;
using BitMobile.Common.StyleSheet;

namespace BitMobile.Common.Controls
{
    public interface ILayoutable : IStyledObject, IDisposable
    {
        string Id { get; set; }
        IRectangle Frame { get; set; }
        ILayoutableContainer Parent { get; set; }
        IStyleKey StyleKey { get; set; }
        IDictionary<Type, IStyle> Styles { get; set; }
        void CreateView();
        void Refresh();
    }
}