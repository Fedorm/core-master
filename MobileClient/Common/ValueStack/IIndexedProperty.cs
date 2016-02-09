namespace BitMobile.Common.ValueStack
{
    public interface IIndexedProperty
    {
        object GetValue(string propertyName);
        bool HasProperty(string propertyName);
    }
}
