using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Telegram.Cryptography;
using Telegram.Helpers;
using Telegram.Math;
using Telegram.Service;
using Telegram.Sessions;
using Telegram.Translation;
using Telegram.TransportLayer;

namespace Telegram.Authorize
{
    internal class AuthKeyCreator
    {
        private readonly Formatter _formatter;
        private byte[] _authKey;
        private BigInteger _newNonce;
        private BigInteger _nonce;
        private BigInteger _serverNonce;

        public AuthKeyCreator(Formatter formatter)
        {
            _formatter = formatter;
        }

        public long InitialSalt { get; private set; }

        public byte[] CreateKey(string address, int port)
        {
            if (GenerateAuthKey(address, port))
                return _authKey;
            return null;
        }

        #region Private Methods

        /// <summary>
        /// Генерация ключа авторизации
        /// </summary>
        /// <returns></returns>
        private bool GenerateAuthKey(string address, int port)
        {
            using (var connection = new TcpConnection(address, port, _formatter))
            {
                connection.Connect(true);

                var pns = new PlainMtProtoSession(connection);

                _nonce = BigInteger.GenerateRandom(128);
                var reqpq = new Combinator("req_pq", _nonce);
                RpcAnswer result = pns.RpcCall(reqpq,
                    "resPQ nonce:int128 server_nonce:int128 pq:string server_public_key_fingerprints:Vector long = ResPQ");
                if (!result.Success) throw new Exception(result.Error.ToString());

                Combinator reqDhParams = ProcessPqAnswer(result.Combinator);

                result = pns.RpcCall(reqDhParams,
                    "server_DH_params_ok nonce:int128 server_nonce:int128 encrypted_answer:string = Server_DH_Params",
                    "server_DH_params_fail nonce:int128 server_nonce:int128 new_nonce_hash:int128 = Server_DH_Params");

                if (result.Combinator.Name == "server_DH_params_ok")
                {
                    Combinator serverDhInnerData = ProcessDhParams(result.Combinator);
                    Combinator setClientDhParams = SetClientDhParams(serverDhInnerData);

                    result = pns.RpcCall(setClientDhParams,
                        "dh_gen_ok nonce:int128 server_nonce:int128 new_nonce_hash1:int128 = Set_client_DH_params_answer",
                        "dh_gen_retry nonce:int128 server_nonce:int128 new_nonce_hash2:int128 = Set_client_DH_params_answer",
                        "dh_gen_fail nonce:int128 server_nonce:int128 new_nonce_hash3:int128 = Set_client_DH_params_answer");

                    Thread.Sleep(100);

                    switch (result.Combinator.Name)
                    {
                        case "dh_gen_ok":
                            InitialSalt = CalculateInitialSalt(_newNonce, _serverNonce);

                            // Проверим new_nonce_hash1
                            bool res = CheckNewNonceHash(result.Combinator.Get<BigInteger>("new_nonce_hash1"), 1);

                            return res;
                        case "dh_gen_retry": // HACK: ретри не реализован
                        case "dh_gen_fail":
                            return false;
                        default:
                            return false;
                    }
                }
                return false;

            }
        }

        /// <summary>
        ///     Формирование соли по схеме substr(new_nonce, 0, 8) XOR substr(server_nonce, 0, 8)
        /// </summary>
        /// <param name="newNonce"></param>
        /// <param name="serverNonce"></param>
        /// <returns></returns>
        private long CalculateInitialSalt(BigInteger newNonce, BigInteger serverNonce)
        {
            var nn = new byte[8];
            var sn = new byte[8];

            Array.Copy(newNonce.GetBytes(), nn, 8);
            Array.Copy(serverNonce.GetBytes(), sn, 8);

            return BitConverter.ToInt32(Aes256IgeManaged.XOR(nn, sn), 0);
        }

        private bool CheckNewNonceHash(BigInteger newNonceHash, byte addVal)
        {
            BigInteger localHash;
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(_newNonce.GetBytes());
                bw.Write(addVal);

                SHA1 sha = SHA1.Create();
                bw.Write(sha.ComputeHash(_authKey), 0, 8);
                byte[] hash = sha.ComputeHash(ms.ToArray());
                ms.SetLength(0);

                bw.Write(hash, hash.Length - 16, 16);

                localHash = new BigInteger(ms.ToArray());
            }

            return localHash == newNonceHash;
        }


        /// <summary>
        ///     Генерация массива случайных байт
        /// </summary>
        /// <param name="need">число элементов</param>
        /// <returns></returns>
        private static byte[] GenerateRandomBytes(long need)
        {
            var array = new byte[need];
            var random = new Random();
            random.NextBytes(array);
            return array;
        }

        /// <summary>
        ///     Разложение на простые сомножетили по методу Ро-Полланда
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private BigInteger RhoPollard(BigInteger n)
        {
            BigInteger x = BigInteger.GenerateRandom(32);
            BigInteger y = 1;

            int i = 0;
            int stage = 2;

            while (n.GCD(x > y ? x - y : y - x) == 1)
            {
                if (i == stage)
                {
                    y = x;
                    stage = stage * 2;
                }
                x = (x * x + 1) % n;
                i = i + 1;
            }
            return n.GCD(x > y ? x - y : y - x);
        }

        /// <summary>
        ///     Простое шифрование RSA (r^e mod m)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] RSAEncrypt(byte[] data)
        {
            const string rsaModulus =
                "C150023E2F70DB7985DED064759CFECF0AF328E69A41DAF4D6F01B538135A6F91F8F8B2A0EC9BA9720CE352EFCF6C5680FFC424BD634864902DE0B4BD6D49F4E580230E3AE97D95C8B19442B3C0A10D8F5633FECEDD6926A7F6DAB0DDB7D457F9EA81B8465FCD6FFFEED114011DF91C059CAEDAF97625F6C96ECC74725556934EF781D866B34F011FCE4D835A090196E9A5F0E4449AF7EB697DDB9076494CA5F81104A305B6DD27665722C46B60E5DF680FB16B210607EF217652E60236C255F6A28315F4083A96791D7214BF64C1DF4FD0DB1944FB26A2A57031B32EEE64AD15A8BA68885CDE74A5BFC920F6ABF59BA5C75506373E7130F9042DA922179251F";
            const string rsaExponent = "010001";

            // HACK: адок
            var m = new BigInteger(BinaryHelper.StringToByteArray(rsaModulus));
            var e = new BigInteger(BinaryHelper.StringToByteArray(rsaExponent));
            var r = new BigInteger(data);
            BigInteger s = r.ModPow(e, m);
            byte[] temp = s.GetBytes();
            return temp;
        }

        /// <summary>
        ///     Создание ключа AES
        /// </summary>
        /// <returns></returns>
        private byte[] CalculateTmpAesKey(BigInteger newNonce, BigInteger serverNonce)
        {
            //tmp_aes_key := SHA1(new_nonce + server_nonce) + substr (SHA1(server_nonce + new_nonce), 0, 12);        

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                SHA1 sha1 = SHA1.Create();

                bw.Write(newNonce.GetBytes());
                bw.Write(serverNonce.GetBytes());

                byte[] sumSha = sha1.ComputeHash(ms.ToArray());

                ms.SetLength(0);
                bw.Write(serverNonce.GetBytes());
                bw.Write(newNonce.GetBytes());

                byte[] reverseSumSha = sha1.ComputeHash(ms.ToArray());

                ms.SetLength(0);
                bw.Write(sumSha);
                bw.Write(reverseSumSha, 0, 12);

                return ms.ToArray();
            }
        }

        /// <summary>
        ///     Создание вектора AES
        /// </summary>
        /// <param name="newNonce"></param>
        /// <param name="serverNonce"></param>
        /// <returns></returns>
        private byte[] CalculateTmpAesIV(BigInteger newNonce, BigInteger serverNonce)
        {
            //tmp_aes_iv := substr (SHA1(server_nonce + new_nonce), 12, 8) + SHA1(new_nonce + new_nonce) + substr (new_nonce, 0, 4);

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                SHA1 sha1 = SHA1.Create();

                bw.Write(serverNonce.GetBytes());
                bw.Write(newNonce.GetBytes());

                byte[] hash1 = sha1.ComputeHash(ms.ToArray());

                ms.SetLength(0);
                bw.Write(newNonce.GetBytes());
                bw.Write(newNonce.GetBytes());

                byte[] hash2 = sha1.ComputeHash(ms.ToArray());

                ms.SetLength(0);
                bw.Write(hash1, 12, 8);
                bw.Write(hash2);
                bw.Write(newNonce.GetBytes(), 0, 4);

                return ms.ToArray();
            }
        }

        #endregion

        #region Auth Generation Methods

        /// <summary>
        /// Генерация клиентского запроса с параметрами Диффи-Хеллмана
        /// </summary>
        /// <param name="serverDh"></param>
        /// <returns></returns>
        private Combinator SetClientDhParams(Combinator serverDh)
        {
            var g = serverDh.Get<int>("g");
            var dhPrime = serverDh.Get<byte[]>("dh_prime");
            var gA = serverDh.Get<byte[]>("g_a");

            var dh = new DiffieHellmanManaged(dhPrime, new BigInteger(g).GetBytes(), 2048);
            // generate the public key of the second DH instance
            byte[] gB = dh.CreateKeyExchange();
            // let the second DH instance compute the shared secret using the first DH public key
            _authKey = dh.DecryptKeyExchange(gA);

            // Сформируем ответ
            // отдаем g_b в BE
            var clientDhInnerData = new Combinator("client_DH_inner_data", _nonce, _serverNonce, (long)0, gB);
            byte[] s = clientDhInnerData.Serialize();

            // Шифрование строки
            var aes = new Aes256IgeManaged(CalculateTmpAesKey(_newNonce, _serverNonce)
                , CalculateTmpAesIV(_newNonce, _serverNonce));

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                SHA1 sha1 = SHA1.Create();
                bw.Write(sha1.ComputeHash(s));
                bw.Write(s);

                var r = new Random();
                while (bw.BaseStream.Length % 16 != 0)
                    bw.Write((byte)r.Next());

                s = aes.Encrypt(ms.ToArray());
            }

            // Сформируем ответ
            var setClientDhParams = new Combinator("set_client_DH_params", _nonce, _serverNonce, s);

            return setClientDhParams;
        }

        /// <summary>
        /// Обработка DH параметров
        /// </summary>
        /// <param name="dhparams"></param>
        /// <returns></returns>
        private Combinator ProcessDhParams(Combinator dhparams)
        {
            var ea = dhparams.Get<byte[]>("encrypted_answer");

            // Обновим server nonce
            _serverNonce = dhparams.Get<BigInteger>("server_nonce");

            // Расшифровка строки
            var aes = new Aes256IgeManaged(CalculateTmpAesKey(_newNonce, _serverNonce),
                CalculateTmpAesIV(_newNonce, _serverNonce));

            byte[] answerWithHash = aes.Decrypt(ea);
            if (answerWithHash.Length % 16 != 0)
                throw new ArgumentException("Неверный ответ внутри сообщения");

            byte[] answer = answerWithHash.Skip(20).ToArray();

            return new Combinator(answer, "Server_DH_inner_data");
        }

        /// <summary>
        /// Обработка входного pq вектора
        /// </summary>
        /// <param name="result">req_DH_params</param>
        /// <returns></returns>
        private Combinator ProcessPqAnswer(Combinator result)
        {
            // Разложение PQ на сомножители
            var u = result.Get<byte[]>("pq");
            var bu = new BigInteger(u); // Входные строки в BE
            BigInteger p = RhoPollard(bu);
            BigInteger q = bu / p;
            if (p > q)
            {
                BigInteger max = q;
                q = p;
                p = max;
            }
            // Генерация encrypted data
            // Сформируем запрос p_q_inner_data#83c95aec pq:string p:string q:string nonce:int128 server_nonce:int128 new_nonce:int256 = P_Q_inner_data
            var nonce = result.Get<BigInteger>("nonce");
            var serverNonce = result.Get<BigInteger>("server_nonce");
            _newNonce = BigInteger.GenerateRandom(256);

            byte[] beP = p.GetBytes(); // p в BE
            byte[] beQ = q.GetBytes(); // q в BE

            var pQInnerData = new Combinator("p_q_inner_data", u, beP, beQ, nonce, serverNonce, _newNonce);

            if (_nonce != nonce)
                throw new ArgumentException("nonce ответа несовпадает");

            SHA1 sha = SHA1.Create();
            byte[] data = pQInnerData.Serialize();
            byte[] hash = sha.ComputeHash(data);
            byte[] dataWithHash;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(hash);
                bw.Write(data);
                bw.Write(GenerateRandomBytes(255 - bw.BaseStream.Position));
                dataWithHash = ms.ToArray();
            }

            byte[] encryptedData = RSAEncrypt(dataWithHash);

            var fingerprint = result.Get<Combinator>("server_public_key_fingerprints").Get<long>(0);

            // HACK: отпечаток ключа
            return new Combinator("req_DH_params", nonce, serverNonce, beP, beQ, fingerprint, encryptedData);
        }

        #endregion
    }
}