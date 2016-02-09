using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Telegram.Cryptography;
using Telegram.Math;

namespace Telegram.Authorize
{
    /// <summary>
    /// </summary>
    internal class EncryptedMessage
    {
        private readonly byte[] _authKey;
        private int _x;

        /// <summary>
        ///     Конструктор по умолчанию
        /// </summary>
        /// <param name="authKey"></param>
        /// <param name="data"></param>
        public EncryptedMessage(byte[] authKey, EncryptedData data, int x)
        {
            SHA1 sha1 = SHA1.Create();
            _authKey = authKey;
            byte[] hash = sha1.ComputeHash(authKey);
            AuthKeyId = BitConverter.ToInt64(hash, hash.Length - 8);

            hash = sha1.ComputeHash(data.SerializeNoPadding());
            var buf = new byte[16];
            Array.Copy(hash, hash.Length - 16, buf, 0, 16);
            MsgKey = new BigInteger(buf);
            _x = x;

            Data = data;
        }

        public EncryptedMessage(byte[] authKey, byte[] plainData)
        {
            _authKey = authKey;
            using (var ms = new MemoryStream(plainData))
            {
                using (var br = new BinaryReader(ms))
                {
                    AuthKeyId = br.ReadInt64();
                    MsgKey = new BigInteger(br.ReadBytes(16));

                    // дешифруем эту дату
                    byte[] aesKey = CalculateAesKey(8, MsgKey.GetBytes());
                    byte[] aesIv = CalculateIV(8, MsgKey.GetBytes());

                    var aesIge = new Aes256IgeManaged(aesKey, aesIv);
                    Data = new EncryptedData(aesIge.Decrypt(br.ReadBytes(plainData.Length - 8 - 16)));
                }
            }
        }

        /// <summary>
        ///     Идентификатор ключа
        /// </summary>
        /// <remarks>
        ///     Младшие 64 бита SHA1 авторизационного ключа, используется для указания того, каким именно ключом зашифровано
        ///     сообщение.
        ///     Ключи должны однозначно задаваться младшими 64 битами своего SHA1; в случае коллизии авторизационный ключ
        ///     перегенерируется.
        ///     Нулевой идентификатор ключа означает, что шифрование не используется; это допускается для ограниченного набора
        ///     типов сообщений,
        ///     используемых при регистрации для создания авторизационного ключа по Диффи-Хелману.
        /// </remarks>
        public Int64 AuthKeyId { get; set; }

        /// <summary>
        ///     Ключ сообщения
        /// </summary>
        /// <remarks>
        ///     Младшие 128 бит SHA1 от шифруемой части сообщения (включая внутренний заголовок, не включая байты выравнивания).
        /// </remarks>
        public BigInteger MsgKey { get; set; }

        public EncryptedData Data { get; set; }

        public int Length
        {
            get { return 8 + 16 + Data.Length; }
        }

        #region Private Methods

        private byte[] CalculateAesKey(int x, byte[] msg_key)
        {
            SHA1 sha = SHA1.Create();
            byte[] sha1_a;
            byte[] sha1_b;
            byte[] sha1_c;

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    // sha1_a = SHA1 (msg_key + substr (auth_key, x, 32));
                    bw.Write(msg_key);
                    bw.Write(_authKey, x, 32);
                    sha1_a = sha.ComputeHash(ms.ToArray());
                    ms.SetLength(0);
                    // sha1_b = SHA1 (substr (auth_key, 32+x, 16) + msg_key + substr (auth_key, 48+x, 16));
                    bw.Write(_authKey, 32 + x, 16);
                    bw.Write(msg_key);
                    bw.Write(_authKey, 48 + x, 16);
                    sha1_b = sha.ComputeHash(ms.ToArray());
                    ms.SetLength(0);
                    // sha1_с = SHA1 (substr (auth_key, 64+x, 32) + msg_key);
                    bw.Write(_authKey, 64 + x, 32);
                    bw.Write(msg_key);
                    sha1_c = sha.ComputeHash(ms.ToArray());
                    ms.SetLength(0);
                    // aes_key = substr (sha1_a, 0, 8) + substr (sha1_b, 8, 12) + substr (sha1_c, 4, 12);
                    bw.Write(sha1_a, 0, 8);
                    bw.Write(sha1_b, 8, 12);
                    bw.Write(sha1_c, 4, 12);
                }
                return ms.ToArray();
            }
        }

        private byte[] CalculateIV(int x, byte[] msg_key)
        {
            SHA1 sha = SHA1.Create();
            byte[] sha1_a;
            byte[] sha1_b;
            byte[] sha1_c;
            byte[] sha1_d;

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    // sha1_a = SHA1 (msg_key + substr (auth_key, x, 32));
                    bw.Write(msg_key);
                    bw.Write(_authKey, x, 32);
                    sha1_a = sha.ComputeHash(ms.ToArray());
                    ms.SetLength(0);
                    // sha1_b = SHA1 (substr (auth_key, 32+x, 16) + msg_key + substr (auth_key, 48+x, 16));
                    bw.Write(_authKey, 32 + x, 16);
                    bw.Write(msg_key);
                    bw.Write(_authKey, 48 + x, 16);
                    sha1_b = sha.ComputeHash(ms.ToArray());
                    ms.SetLength(0);
                    // sha1_с = SHA1 (substr (auth_key, 64+x, 32) + msg_key);
                    bw.Write(_authKey, 64 + x, 32);
                    bw.Write(msg_key);
                    sha1_c = sha.ComputeHash(ms.ToArray());
                    ms.SetLength(0);
                    // sha1_d = SHA1 (msg_key + substr (auth_key, 96+x, 32));
                    bw.Write(msg_key);
                    bw.Write(_authKey, 96 + x, 32);
                    sha1_d = sha.ComputeHash(ms.ToArray());
                    ms.SetLength(0);
                    // aes_iv = substr (sha1_a, 8, 12) + substr (sha1_b, 0, 8) + substr (sha1_c, 16, 4) + substr (sha1_d, 0, 8);
                    bw.Write(sha1_a, 8, 12);
                    bw.Write(sha1_b, 0, 8);
                    bw.Write(sha1_c, 16, 4);
                    bw.Write(sha1_d, 0, 8);
                }
                return ms.ToArray();
            }
        }

        #endregion

        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(AuthKeyId);
                    bw.Write(MsgKey.GetBytes());

                    byte[] aesKey = CalculateAesKey(0, MsgKey.GetBytes());
                    byte[] aesIV = CalculateIV(0, MsgKey.GetBytes());

                    var aesIge = new Aes256IgeManaged(aesKey, aesIV);
                    bw.Write(aesIge.Encrypt(Data.Serialize()));
                }
                return ms.ToArray();
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("AuthKeyId: {0}\n", AuthKeyId.ToString("X"));
            sb.AppendFormat("MsgKey: {0}\n", MsgKey.ToString(16));
            sb.AppendLine("-- Data --");
            sb.Append(Data);

            return sb.ToString();
        }

        internal int GetConstructorCrc()
        {
            return BitConverter.ToInt32(Data.MessageData, 0);
        }
    }
}