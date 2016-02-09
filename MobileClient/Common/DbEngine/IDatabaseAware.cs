namespace BitMobile.Common.DbEngine
{
    public interface IDatabaseAware
    {
        void SetDatabase(IDatabase database);
    }
}
