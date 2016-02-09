using System;
using System.Collections.Generic;
using System.IO;
using BitMobile.Common.Controls;

namespace BitMobile.Common.StyleSheet
{
    public interface IStyleSheet : IDisposable
    {
        IStyleSheetHelper Helper { get; }
        IDictionary<Type, IStyle> GetStyles(object obj);
        void Assign(ILayoutable root);
        void Load(Stream stream);
        IDictionary<Type, IStyle> StylesIntersection(IDictionary<Type, IStyle> oldStyles, IDictionary<Type, IStyle> newStyles);
        T GetCache<T>() where T : class, IDisposable;
        void SetCache<T>(T cache) where T : class, IDisposable;
        void RemoveCache();
    }
}
