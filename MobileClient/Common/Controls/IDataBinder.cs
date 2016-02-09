namespace BitMobile.Common.Controls
{
    public interface IDataBinder
    {
        void ControlChanged(object value);
        bool IsNumeric();
        string ToString();
    }
}
