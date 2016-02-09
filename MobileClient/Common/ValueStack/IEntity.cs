using BitMobile.Common.Entites;
using BitMobile.Common.Log;

namespace BitMobile.Common.ValueStack
{
    public interface IEntity : IIndexedProperty, ILoggable
    {
        void SetValue(string propertyName, object value);
        IEntityType EntityType { get; }
    }
}