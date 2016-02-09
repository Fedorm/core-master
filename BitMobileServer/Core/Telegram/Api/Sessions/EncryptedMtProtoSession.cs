using System;
using System.Security.Cryptography;
using Telegram.Authorize;
using Telegram.TransportLayer;

namespace Telegram.Sessions
{
    internal class EncryptedMtProtoSession
    {
        private readonly byte[] _authKey;
        private readonly Random _r = new Random();
        private int _messageSeqNumber;

        public EncryptedMtProtoSession(byte[] authKey, Int64 salt)
        {
            _authKey = authKey;
            Salt = salt;

            SessionId = LongRandom();
        }

        public long SessionId { get; private set; }

        public Int64 Salt { get; set; }

        // msg_container#73f1f8dc messages:vector message = MessageContainer;
        // message msg_id:long seqno:int bytes:int body:Object = Message;
        public long GetNextMessageId()
        {
            long ts = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds);
            return (ts * 4294967 + (ts * 296 / 1000)) & ~3L;
        }

        public int GetNextSeqNo()
        {
            int result = _messageSeqNumber * 2 + 1;
            _messageSeqNumber++;
            return result;
        }

        public EncryptedMessage PrepareRpcCall(SessionContainer request)
        {
            SHA1.Create();

            var encData = new EncryptedData
            {
                Salt = Salt,
                SessionId = SessionId,
                MessageId = GetNextMessageId(),
                SeqNo = GetNextSeqNo(),
                MessageData = request.Combinator.Serialize()
            };
            
            encData.MessageDataLength = encData.MessageData.Length;
            return new EncryptedMessage(_authKey, encData, 0);
        }

        private long LongRandom()
        {
            var buf = new byte[8];
            _r.NextBytes(buf);
            return BitConverter.ToInt64(buf, 0);
        }
    } // class
} // ns