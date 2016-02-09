namespace Telegram
{
    public interface ITelegramPersist
    {
        void Save(string phone, byte[] authKey, long serverSalt);
        bool TryGet(string phone, out byte[] authKey, out long serverSalt);
    }
}
