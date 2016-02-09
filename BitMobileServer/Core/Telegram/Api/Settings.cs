namespace Telegram.Api
{
    public class Settings
    {
        /// <summary>
        /// Stored auth key
        /// </summary>
        public byte[] AuthKey { get; set; }

        /// <summary>
        /// Server salt
        /// </summary>
        public long NonceNewNonceXor { get; set; }
    }
}