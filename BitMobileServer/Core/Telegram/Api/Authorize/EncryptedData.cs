using System;
using System.IO;
using System.Text;
using Telegram.Helpers;

namespace Telegram.Authorize
{
    /// <summary>
    /// </summary>
    internal class EncryptedData
    {
        public EncryptedData(byte[] plainData)
        {
            using (var ms = new MemoryStream(plainData))
            {
                using (var br = new BinaryReader(ms))
                {
                    Salt = br.ReadInt64();
                    SessionId = br.ReadInt64();
                    MessageId = br.ReadInt64();
                    SeqNo = br.ReadInt32();
                    MessageDataLength = br.ReadInt32();
                    if (MessageDataLength < 0 || MessageDataLength > ms.Length - ms.Position)
                        throw new DecodeException("Incorrect message length: " + MessageDataLength);
                    
                    MessageData = br.ReadBytes(MessageDataLength);
                }
            }
        }

        public EncryptedData()
        {
        }

        /// <summary>
        ///     Серверная соль
        /// </summary>
        /// <remarks>
        ///     64-битное число, время от времени (скажем, раз в сутки) изменяемое (отдельно для каждой сессии) по инициативе
        ///     сервера.
        ///     После этого все сообщения должны содержать новую соль (хотя в течение 300 секунд еще принимаются сообщения и со
        ///     старой солью).
        ///     Нужна для защиты от replay-атак, и некоторых фокусов, связанных с переводом часов на клиенте в далекое будущее.
        /// </remarks>
        public Int64 Salt { get; set; }

        /// <summary>
        ///     Сессия
        /// </summary>
        /// <remarks>
        ///     64-битное число, генерируемое клиентом для различения отдельных сессий (например, различных экземпляров приложения,
        ///     запущенных с одним и тем же авторизационным ключом).
        ///     Сессия вместе с идентификатором ключа соответствуют экземпляру приложения. Сервер может хранить состояние,
        ///     соответствующее сессии.
        ///     Ни при каких обстоятельствах сообщение, предназначенное для одной сессии, не может быть отправлено в другую сессию.
        /// </remarks>
        public Int64 SessionId { get; set; }

        /// <summary>
        ///     Идентификатор сообщения (msg_id)
        /// </summary>
        /// <remarks>
        ///     64-битное число, уникально идентифицирующее сообщение в рамках сессии.
        ///     Идентификаторы клиентских сообщений делятся на 4, серверных - дают остаток 1 от деления на 4, если они являются
        ///     ответом на клиентское сообщение, и остаток 3 иначе.
        ///     Идентификаторы клиентских сообщений должны монотонно расти (в пределах одной сессии), равно как и серверных
        ///     сообщений; они должны примерно равняться unixtime * 2^32.
        ///     Таким образом, идентификатор сообщения примерно определяет время его создания.
        ///     Сообщение не принимается, если прошло более 300 секунд после его создания или за 30 секунд до его создания (это
        ///     нужно для защиты от replay-атак);
        ///     в этом случае оно должно быть перепослано с другим идентификатором (или помещено в контейнер с большим
        ///     идентификатором).
        ///     Идентификатор сообщения-контейнера должен быть строго больше идентификаторов содержащихся в нем сообщений.
        /// </remarks>
        public Int64 MessageId { get; set; }

        /// <summary>
        ///     Порядковый номер сообщения (msg_seqno)
        /// </summary>
        /// <remarks>
        ///     32-битное число, равное удвоенному количеству “содержательных” сообщений (т.е. нуждающихся в подтверждении, и, в
        ///     частности, не являющихся контейнерами),
        ///     созданных отправителем до данного сообщения, и увеличенному затем на единицу, если текущее сообщение является
        ///     содержательным.
        ///     Контейнер всегда генерируется после всего, что в нем содержится, поэтому его порядковый номер не меньше порядковых
        ///     номеров сообщений, что есть внутри него.
        /// </remarks>
        public Int32 SeqNo { get; set; }

        /// <summary>
        ///     Длина сообщения
        /// </summary>
        public Int32 MessageDataLength { get; set; }

        /// <summary>
        ///     Сообщение
        /// </summary>
        public byte[] MessageData { get; set; }

        /// <summary>
        ///     padding 0..15
        /// </summary>
        public byte[] Padding { get; set; }

        public int Length
        {
            get { return 8 + 8 + 8 + 4 + 4 + MessageData.Length + Padding.Length; }
        }


        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("Salt: {0}\n", Salt.ToString("X"));
            sb.AppendFormat("SessionId: {0}\n", SessionId.ToString("X"));
            sb.AppendFormat("SeqNo: {0}\n", SeqNo);
            sb.AppendFormat("MessageId: {0}\n", MessageId.ToString("X"));
            sb.AppendFormat("MessageDataLength: {0}\n", MessageDataLength);
            sb.AppendFormat("Plain MessageData: {0}\n", BinaryHelper.ByteToHexBitFiddle(MessageData));

            return sb.ToString();
        }

        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(Salt);
                    bw.Write(SessionId);
                    bw.Write(MessageId);
                    //bw.Write(BitConverter.GetBytes(this.MessageId).Reverse().ToArray());
                    bw.Write(SeqNo);
                    bw.Write(MessageDataLength);
                    bw.Write(MessageData);

                    var r = new Random();
                    while (bw.BaseStream.Length % 16 != 0)
                    {
                        bw.Write((byte)r.Next());
                    }
                }
                return ms.ToArray();
            }
        }


        internal byte[] SerializeNoPadding()
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(Salt);
                    bw.Write(SessionId);
                    bw.Write(MessageId);
                    //bw.Write(BitConverter.GetBytes(this.MessageId).Reverse().ToArray());
                    bw.Write(SeqNo);
                    bw.Write(MessageDataLength);
                    bw.Write(MessageData);
                }
                return ms.ToArray();
            }
        }
    }
}