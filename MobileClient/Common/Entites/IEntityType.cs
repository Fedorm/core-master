using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Common.Entites
{
    public interface IEntityType
    {
        string Schema { get; }
        string Name { get; }
        string TableName { get; }
        string TypeName { get; }
        ICollection<IEntityField> Fields { get; }
        string IdFieldName { get; }
        bool IsTable { get; }
        IEntityField GetField(string name);
        int GetPropertyIndex(string name);
        string[] GetProperties();
        string[] GetColumns();
        bool Exists(string propertyName);
        Type GetPropertyType(string propertyName);
        bool IsPrimaryKey(string propertyName);
        bool IsIndexed(string propertyName);
    }
}
