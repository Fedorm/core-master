using System;
using System.IO;
using System.Linq;
using System.Text;
using Telegram.Helpers;
using Telegram.Math;

namespace Telegram.Translation
{
    internal class Formatter : IFormatter<byte[]>
    {
        public byte[] FromInt32(int input)
        {
            return BitConverter.GetBytes(input);
        }

        public byte[] FromInt64(long input)
        {
            return BitConverter.GetBytes(input);
        }

        public byte[] FromInt128(BigInteger input)
        {
            return FromBigInt(input, 16);
        }

        public byte[] FromInt256(BigInteger input)
        {
            return FromBigInt(input, 32);
        }

        public byte[] FromString(string input)
        {
            return SerializeString(RawString(input));
        }

        public byte[] FromBytes(byte[] input)
        {
            return SerializeString(input);
        }

        public int ReadInt32(byte[] input, ref int offset)
        {
            byte[] b = Read(input, 4, ref offset);
            return BitConverter.ToInt32(b, 0);
        }

        public long ReadInt64(byte[] input, ref int offset)
        {
            byte[] b = Read(input, 8, ref offset);
            return BitConverter.ToInt64(b, 0);
        }

        public BigInteger ReadInt128(byte[] input, ref int offset)
        {
            byte[] b = Read(input, 16, ref offset);
            return new BigInteger(b);
        }

        public BigInteger ReadInt256(byte[] input, ref int offset)
        {
            byte[] b = Read(input, 32, ref offset);
            return new BigInteger(b);
        }

        public string ReadString(byte[] input, ref int offset)
        {
            return Encoding.UTF8.GetString(ReadBytes(input, ref offset));
        }

        public byte[] ReadBytes(byte[] input, ref int offset)
        {
            int length;
            int padding;

            byte markByte = input[offset++];
            // Выясним длину строки
            if (markByte <= 253)
            {
                length = markByte;
                padding = CalculatePadding(length + 1);
            }
            else
            {
                if (markByte != 254)
                    throw new ArgumentException("Неверный формат длинного string, 1 байт равен " +
                                                markByte.ToString("X"));
                byte[] num = Read(input, 3, ref offset);
                var prenum = new byte[4];
                Array.Copy(num, prenum, 3);
                length = BitConverter.ToInt32(prenum, 0);
                padding = CalculatePadding(length + 4);
            }

            byte[] result = Read(input, length, ref offset);
            offset += padding;
            return result;
        }

        public byte[] ReadToEnd(byte[] input, ref int offset)
        {
            int count = input.Length - offset;
            var result = new byte[count];
            Array.Copy(input, offset, result, 0, count);
            offset = input.Length;
            return result;
        }

        public byte[] Merge(byte[] first, byte[] second)
        {
            var result = new byte[first.Length + second.Length];
            Array.Copy(first, result, first.Length);
            Array.Copy(second, 0, result, first.Length, second.Length);
            return result;
        }

        public byte[] Merge(byte[] first, byte[][] second)
        {
            int secondLength = second.Sum(val => val.Length);
            var result = new byte[first.Length + secondLength];

            Array.Copy(first, result, first.Length);
            int offset = first.Length;
            foreach (var s in second)
            {
                Array.Copy(s, 0, result, offset, s.Length);
                offset += s.Length;
            }
            return result;
        }

        public byte[] RawString(string input)
        {
            return input.StartsWith("0x")
                ? BinaryHelper.StringToByteArray(input.Replace("0x", ""))
                : Encoding.UTF8.GetBytes(input);
        }

        private static byte[] FromBigInt(BigInteger input, int length)
        {
            byte[] raw = input.GetBytes();
            var result = new byte[length];
            Array.Copy(raw, result, length);
            return result;
        }

        private byte[] SerializeString(byte[] stringRaw)
        {
            int strLen = stringRaw.Length;

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    if (strLen <= 253)
                    {
                        // Если L меньше или равно 253, то кодируется один байт L, затем L байтов строки, затем от 0 до 3 символов с кодом 0, 
                        // чтобы общая длина значения делилась на 4, после чего все это интерпретируется как последовательность из int(L/4) + 1 32-битных чисел. 
                        bw.Write((byte) strLen);
                        bw.Write(stringRaw);
                        for (int i = 0; i < CalculatePadding(strLen + 1); i++)
                            bw.Write((byte) 0x0);
                    }
                    else
                    {
                        // Если же L >= 254, то кодируется байт 254, затем — 3 байта с длиной строки L, 
                        // затем — L байтов строки, затем - от 0 до 3 нулевых байтов выравнивания.
                        //HACK: Какое выравнивание строки??! НЕВЕРНО ВПИСАНА ДЛИНА
                        byte[] b = BitConverter.GetBytes(strLen);
                        //b[0] = 254;s
                        bw.Write((byte) 254);
                        bw.Write(b, 0, 3);
                        bw.Write(stringRaw);
                        for (int i = 0; i < CalculatePadding(strLen); i++)
                            bw.Write((byte) 0x0);
                    }
                    return ms.ToArray();
                }
            }
        }

        private static int CalculatePadding(int length)
        {
            return ((((length)%4) == 0) ? 0 : 4 - (length)%4);
        }

        private byte[] Read(byte[] input, int count, ref int offset)
        {
            var result = new byte[count];
            for (int i = 0; i < count; i++)
                result[i] = input[offset + i];
            offset += count;
            return result;
        }
    }
}