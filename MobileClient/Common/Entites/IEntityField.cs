using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Common.Entites
{
    public interface IEntityField
    {
        string Name { get; }
        Type Type { get; }
        bool KeyField { get; }
        bool AllowNull { get; }
        int Index { get; }
        string DbRefTable { get; }
        bool IsRef { get; }
    }
}
