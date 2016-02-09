using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Telegram.Authorize;
using Telegram.Cryptography;
using Telegram.Service;
using Telegram.TransportLayer;

namespace Telegram.Sessions
{
    internal class PlainMtProtoSession
    {
        private readonly TcpConnection _connection;
        private int _packetNumber;

        public PlainMtProtoSession(TcpConnection connection)
        {
            _connection = connection;
            _packetNumber = 0;
        }

        public RpcAnswer RpcCall(Combinator combinator, params string[] expectedAnswers)
        {
            Trace.TraceInformation("#Send plain: {0}", combinator);

            var pm = new PlainMessage(0, combinator);

            var transport = new TcpTransport(_connection.PacketNumber++, pm.Serialize());

            byte[] responseb = _connection.ExchangeWithServer(transport.Serialize());

            TcpTransport answer;
            using (var ms = new MemoryStream(responseb))
                answer = new TcpTransport(ms);
            uint constructor = PlainMessage.ExtractConstructor(answer.Payload);

            new Crc32();

            foreach (string item in expectedAnswers)
            {
                uint crc32 = Crc32.Compute(Encoding.UTF8.GetBytes(item));
                if (crc32 == constructor)
                {
                    var resultCombinator = new PlainMessage(answer.Payload, item.Split(' ').Last().Trim()).Combinator;
                    Trace.TraceInformation("#Recieve plain: {0}", resultCombinator);
                    return new RpcAnswer(resultCombinator);
                }
            }
            throw new ArgumentException("unexpected answer");
        }
    }
}