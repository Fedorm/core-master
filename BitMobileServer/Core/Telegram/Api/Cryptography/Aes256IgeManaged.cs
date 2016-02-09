using System;
using System.IO;
using System.Security.Cryptography;

namespace Telegram.Cryptography
{
    /// <summary>
    ///     Класс шифровальщик
    /// </summary>
    internal class Aes256IgeManaged
    {
        private byte[] _iv1;
        private byte[] _iv2;
        private byte[] _key;

        public Aes256IgeManaged(byte[] key, byte[] iv)
        {
            _key = key;

            _iv1 = new byte[iv.Length/2];
            _iv2 = new byte[iv.Length/2];
            Array.Copy(iv, 0, _iv1, 0, _iv1.Length);
            Array.Copy(iv, iv.Length/2, _iv2, 0, _iv2.Length);
        }

        public Aes256IgeManaged()
        {
        }

        #region Public Methods

        /// <summary>
        ///     Дешифровка
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns></returns>
        public byte[] Decrypt(byte[] encryptedString)
        {
            using (var am = new AesManaged())
            {
                am.Mode = CipherMode.ECB;
                am.KeySize = _key.Length*8;
                am.Padding = PaddingMode.None;
                am.IV = _iv1;
                am.Key = _key;

                int blockSize = am.BlockSize/8;

                var xPrev = new byte[blockSize];
                ;
                Buffer.BlockCopy(_iv1, 0, xPrev, 0, blockSize);
                var yPrev = new byte[blockSize];
                Buffer.BlockCopy(_iv2, 0, yPrev, 0, blockSize);

                var decrypted = new MemoryStream();
                try
                {
                    using (var bw = new BinaryWriter(decrypted))
                    {
                        var y = new byte[blockSize];
                        var x = new byte[blockSize];

                        ICryptoTransform decryptor = am.CreateDecryptor();

                        for (int i = 0; i < encryptedString.Length; i += blockSize)
                        {
                            Buffer.BlockCopy(encryptedString, i, x, 0, blockSize);
                            y = XOR(decryptor.TransformFinalBlock(XOR(x, yPrev), 0, blockSize), xPrev);

                            Buffer.BlockCopy(x, 0, xPrev, 0, blockSize);
                            Buffer.BlockCopy(y, 0, yPrev, 0, blockSize);

                            bw.Write(y);
                        } // for
                    }
                    return decrypted.ToArray();
                }
                finally
                {
                    if (decrypted != null)
                        decrypted.Dispose();
                }
            } // using AesManaged  			
        }

        /// <summary>
        ///     Дешифровка строки AES-256 ECB IGE No Padding
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <param name="tmp_aes_key"></param>
        /// <param name="tmp_aes_iv1"></param>
        /// <param name="tmp_aes_iv2"></param>
        /// <returns></returns>
        public byte[] AES256IgeDecrypt(byte[] encryptedString, byte[] tmp_aes_key, byte[] tmp_aes_iv1,
            byte[] tmp_aes_iv2)
        {
            _key = tmp_aes_key;
            _iv1 = tmp_aes_iv1;
            _iv2 = tmp_aes_iv2;
            return Decrypt(encryptedString);
        }

        /// <summary>
        ///     Шифрование строки AES-256 ECB IGE No Padding
        /// </summary>
        /// <param name="plainString"></param>
        /// <returns></returns>
        public byte[] Encrypt(byte[] plainString)
        {
            using (var am = new AesManaged())
            {
                am.Mode = CipherMode.ECB;
                am.KeySize = _key.Length*8;
                am.Padding = PaddingMode.None;
                am.IV = _iv1;
                am.Key = _key;

                int blockSize = am.BlockSize/8;

                var xPrev = new byte[blockSize];
                ;
                Buffer.BlockCopy(_iv2, 0, xPrev, 0, blockSize);
                var yPrev = new byte[blockSize];
                Buffer.BlockCopy(_iv1, 0, yPrev, 0, blockSize);

                using (var encrypted = new MemoryStream())
                {
                    using (var bw = new BinaryWriter(encrypted))
                    {
                        var y = new byte[blockSize];
                        var x = new byte[blockSize];

                        ICryptoTransform encryptor = am.CreateEncryptor();

                        for (int i = 0; i < plainString.Length; i += blockSize)
                        {
                            Buffer.BlockCopy(plainString, i, x, 0, blockSize);
                            y = XOR(encryptor.TransformFinalBlock(XOR(x, yPrev), 0, blockSize), xPrev);

                            Buffer.BlockCopy(x, 0, xPrev, 0, blockSize);
                            Buffer.BlockCopy(y, 0, yPrev, 0, blockSize);

                            bw.Write(y);
                        } // for
                    } // using BinaryWriter 
                    return encrypted.ToArray();
                } // using MemoryStream 
            } // using AesManaged  			
        }


        /// <summary>
        ///     Шифрование строки AES-256 ECB IGE No Padding
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <param name="tmp_aes_key"></param>
        /// <param name="tmp_aes_iv1"></param>
        /// <param name="tmp_aes_iv2"></param>
        /// <returns></returns>
        public byte[] AES256IgeEncrypt(byte[] encryptedString, byte[] tmp_aes_key, byte[] tmp_aes_iv1,
            byte[] tmp_aes_iv2)
        {
            _key = tmp_aes_key;
            _iv1 = tmp_aes_iv1;
            _iv2 = tmp_aes_iv2;
            return Encrypt(encryptedString);
        }

        #endregion

        public static byte[] XOR(byte[] buffer1, byte[] buffer2)
        {
            var result = new byte[buffer1.Length];
            for (int i = 0; i < buffer1.Length; i++)
                result[i] = (byte) (buffer1[i] ^ buffer2[i]);
            return result;
        }
    }
}