using System;
using System.IO;

namespace Telegram.Authorize
{
    /// <summary>
    ///     Незашифрованные сообщения
    /// </summary>
    /// <remarks>
    ///     Для создания авторизационного ключа, а также для синхронизации времени могут использоваться специальные
    ///     незашифрованные сообщения.
    ///     Они начинаются с auth_key_id = 0 (64 бита), что означает, что auth_key нет.
    ///     Далее идет непосредственно тело сообщения в сериализованном формате, без внешних или внутренних заголовков.
    ///     Перед телом сообщения добавляется его идентификатор (64 бита) и длина тела в байтах (32 бита).
    ///     Только очень ограниченное количество типов специальных сообщений могут передаваться незашифрованными.
    /// </remarks>
    class PlainMessage
    {
        // msg_container#73f1f8dc messages:vector message = MessageContainer;
        // message msg_id:long seqno:int bytes:int body:Object = Message;

        public PlainMessage(Int64 authKeyId, Combinator combinator)
        {
            AuthKeyId = authKeyId;
            MessageId = GetNextMessageId();
            Combinator = combinator;
        }

        public PlainMessage(byte[] raw, string type)
        {
            using (var br = new BinaryReader(new MemoryStream(raw)))
            {
                AuthKeyId = br.ReadInt64();
                MessageId = br.ReadInt64();
                int length = br.ReadInt32();
                Combinator = new Combinator(br.ReadBytes(length), type);
            }
        }

        public Int64 AuthKeyId { get; set; }
        public Int64 MessageId { get; set; }
        public Combinator Combinator { get; set; }

        public long GetNextMessageId()
        {
            long ts = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds);
            return (ts*4294967 + (ts*296/1000)) & ~3L;
        }

        public byte[] Serialize()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(AuthKeyId);
                writer.Write(BitConverter.GetBytes(MessageId));
                byte[] raw = Combinator.Serialize();
                writer.Write(raw.Length);
                writer.Write(raw);
                return stream.ToArray();
            }
        }

        internal static uint ExtractConstructor(byte[] payload)
        {
            return BitConverter.ToUInt32(payload, 20);
        }
    }
}