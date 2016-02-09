using System;
using System.IO;
using System.Linq;
using Telegram.Cryptography;

namespace Telegram.TransportLayer
{
    internal class TcpTransport
    {
        public TcpTransport(Int32 packetNumber, byte[] payload)
        {
            PacketLength = payload.Length + 12;
            PacketNumber = packetNumber;
            Payload = payload;
        }

        public TcpTransport(MemoryStream ms)
        {
            var br = new BinaryReader(ms);
            PacketLength = br.ReadInt32();
            PacketNumber = br.ReadInt32();
            Payload = br.ReadBytes(PacketLength - 12);
            CRC32 = br.ReadUInt32();
        }

        /// <summary>
        ///     4 байта длины
        /// </summary>
        /// <remarks>
        ///     включая длину, порядковый номер и CRC32; всегда делится на четыре
        /// </remarks>
        private Int32 PacketLength { get; set; }

        /// <summary>
        ///     4 байта с порядковым номером пакета внутри данного tcp-соединения
        /// </summary>
        /// <remarks>
        ///     (первый отправленный пакет помечается 0, следующий - 1 и т.д.)
        /// </remarks>
        public Int32 PacketNumber { get; private set; }

        /// <summary>
        ///     Полезная нагрузка
        /// </summary>
        public byte[] Payload { get; set; }

        /// <summary>
        ///     4 байта CRC32
        /// </summary>
        /// <remarks>
        ///     длины, порядкового номера и полезной нагрузки вместе
        /// </remarks>
        private UInt32 CRC32 { get; set; }

        public int Length
        {
            get { return Payload.Length + 12; }
        }

        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    var crc = new Crc32();
                    PacketLength = Length;
                    bw.Write(PacketLength);
                    bw.Write(PacketNumber);
                    bw.Write(Payload, 0, Payload.Length);
                    bw.Write(crc.ComputeHash(ms.ToArray(), 0, 4 + 4 + Payload.Length).Reverse().ToArray());
                }
                return ms.ToArray();
            }
        }
    }
}