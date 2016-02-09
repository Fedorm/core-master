namespace BitMobile.Common.DbEngine
{
    public interface IDbRecordsetEx : IDbRecordset
    {
        void First();
        int Count();
    }
}
