using BitMobile.Common.Entites;

namespace BitMobile.ValueStack
{
    public interface IEntity : IIndexedProperty
    {
        void SetValue(string propertyName, object value);
        EntityType EntityType { get; }
    }
}