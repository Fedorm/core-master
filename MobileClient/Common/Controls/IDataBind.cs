namespace BitMobile.Common.Controls
{
    public interface IDataBind
    {
        IDataBinder Value { get; set; }
        void DataBind();
    }
}