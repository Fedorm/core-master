using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Telegram.Authorize;
using Telegram.Helpers;
using Telegram.Schema;
using Telegram.Service;
using Telegram.Sessions;
using Telegram.Translation;
using Telegram.TransportLayer;

namespace Telegram.Api
{
    class Provider
    {
        private static readonly AutoResetEvent RpcSync = new AutoResetEvent(true);

        private readonly string[] _systemCalls =
        {
            "bad_server_salt",
            "msgs_ack",
            "bad_msg_notification",
            "new_session_created"
        };

        private readonly State _updatingState = new State();
        private readonly string _address;
        private readonly int _port;
        private readonly string _phone;
        private readonly ITelegramPersist _persist;
        private readonly Formatter _formatter;
        private readonly Settings _settings;
        private TcpConnection _connection;
        private EncryptedMtProtoSession _session;
        private bool _authorized;

        public Provider(ApiSchema schema, string address, int port, string phone, ITelegramPersist persist)
        {
            _address = address;
            _port = port;
            _phone = phone;
            _persist = persist;
            _formatter = new Formatter();
            Combinator.Setup(schema, _formatter);

            _settings = LoadSettings();

            _connection = new TcpConnection(_address, _port, _formatter);
        }

        public event Action<Combinator> Updates = combinator => { };
        public event Action<Combinator> Difference = combinator => { };

        public bool CheckAndGenerateAuth()
        {
            var akc = new AuthKeyCreator(_formatter);
            _settings.AuthKey = akc.CreateKey(_address, _port);
            _settings.NonceNewNonceXor = akc.InitialSalt;
            SaveSettings(_settings);

            return _settings.AuthKey != null;
        }

        public bool Authorized
        {
            get
            {
                if (_authorized)
                    return true;

                if (_settings.AuthKey == null || _settings.AuthKey.Length == 0)
                    return false;

                RpcAnswer answer = Ping();
                if (!answer.Success)
                {
                    if (answer.Error.ErrorCode == 401)
                        return false;
                    throw new Exception(answer.Error.ToString());
                }
                _authorized = true;
                return true;
            }
            set { _authorized = value; }
        }

        public bool InitConnection(int apiId, string device, string system, string app, string lang, int layer)
        {
            var combinator = new Combinator("initConnection", apiId, device, system, app, lang
                , new Combinator("invokeWithLayer", layer, new Combinator("updates.getState")));

            RpcAnswer answer = PerformRpcCall(combinator);
            if (answer.Success)
            {
                _updatingState.Update(answer.Combinator);
                _authorized = true;
                return true;
            }
            _authorized = false;

            if (answer.Error.ErrorCode == 401)
                return false;
            throw new Exception(answer.Error.ToString());
        }

        public RpcAnswer PerformRpcCall(Combinator request, bool reconnect = true)
        {
            if (_connection == null)
                throw new Exception("Client not connected");

            RpcSync.WaitOne();
            try
            {
                if (_connection.Connect(reconnect))
                    _session = new EncryptedMtProtoSession(_settings.AuthKey, _settings.NonceNewNonceXor);

                RpcAnswer answer = RpcCall(request);
                if (answer.Combinator != null)
                    _updatingState.Update(answer.Combinator);

                return answer;
            }
            finally
            {
                RpcSync.Set();
            }
        }

        public RpcAnswer Ping()
        {
            return PerformRpcCall(new Combinator("ping", (long)42), false);
        }

        public void UpdateState()
        {
            RpcAnswer answer = PerformRpcCall(new Combinator("updates.getState"));
            if (answer.Success)
                _updatingState.Update(answer.Combinator);
        }

        private RpcAnswer RpcCall(Combinator request)
        {
            List<RpcAnswer> answers = null;
            Exception innerException = null;
            for (int i = 0; i < 3; i++)
            {
                innerException = null;
                try
                {
                    IEnumerable<Combinator> response = Exchange(request);

                    answers = ProcessAnswers(request, response);

                    if (answers.Count > 0)
                        break;
                }
                catch (DecodeException e)
                {
                    innerException = e;
                    Trace.TraceError(e.Message);
                }
            }

            if (innerException != null)
                throw new AggregateException("Exception during rpc call", innerException);

            if (answers == null)
                throw new Exception("Empty answers");

            if (answers.Count > 1 && answers.Any(val => val.Combinator != null && val.Combinator.Descriptor.type != "Pong"))
                throw new Exception(string.Format("Multiple answers: {0}"
                    , answers.Aggregate("", (cur, val) => cur + "\r" + val.ToString())));

            return answers.Last();
        }

        private IEnumerable<Combinator> Exchange(Combinator combinator)
        {
            _session.Salt = _settings.NonceNewNonceXor;
            var oc = new SessionContainer(_session.SessionId, combinator);

            EncryptedMessage encMessage = _session.PrepareRpcCall(oc);
            var call = new TcpTransport(_connection.PacketNumber++, encMessage.Serialize());

            _connection.Write(call.Serialize());
            Trace.TraceInformation("#Send: {0}", combinator);

            var buffer = _connection.Read();
            if (buffer.Length == 0)
                throw new DecodeException("Response is empty");

            var result = new List<Combinator>();// ReSharper disable once LoopCanBeConvertedToQuery
            foreach (SessionContainer container in ProcessInputBuffer(buffer))
            {
                Combinator c = Unwrap(container.Combinator, container.SessionId, _session.SessionId, combinator.Descriptor.type);
                if (c != null)
                    result.Add(c);
            }
            return result;
        }

        private List<RpcAnswer> ProcessAnswers(Combinator request, IEnumerable<Combinator> response)
        {
            var answers = new List<RpcAnswer>();
            foreach (Combinator combinator in response)
            {
                if (_systemCalls.Contains(combinator.Name))
                {
                    Trace.TraceInformation("#System: {0}", combinator);
                    if (combinator.Name == "bad_server_salt")
                    {
                        _settings.NonceNewNonceXor = combinator.Get<long>("new_server_salt");
                        SaveSettings(_settings);
                        RpcAnswer result = RpcCall(request);
                        answers.Add(result);
                    }
                }
                else if (combinator.Descriptor.type == "Updates")
                {
                    Trace.TraceInformation("#Update: {0}", combinator);
                    ProcessUpdates(combinator);
                }
                else
                {
                    Trace.TraceInformation("#Recieve: {0}", combinator);
                    // todo: проверять тип комбинаторов. Учесть: rpc_error, X, Vector t, etc.
                    answers.Add(new RpcAnswer(combinator));
                }

            }
            return answers;
        }

        private IEnumerable<SessionContainer> ProcessInputBuffer(byte[] buffer)
        {
            var transports = new List<TcpTransport>();

            using (var ms = new MemoryStream(buffer))
                // снимем tcp обертку
                while (ms.Position < ms.Length)
                    transports.Add(new TcpTransport(ms));

            var answers = new List<SessionContainer>();
            foreach (TcpTransport item in transports)
                answers.AddRange(ExtractCombinators(item));

            return answers;
        }

        private Combinator Unwrap(Combinator response, long responseId, long sessionId, string type)
        {
            switch (response.Name)
            {
                case "gzip_packed":
                    byte[] packedData = response.Get<byte[]>("packed_data");
                    using (var gz = new GZipStream(new MemoryStream(packedData, 0, packedData.Length), CompressionMode.Decompress, false))
                        response = new Combinator(new BinaryReader(gz).ReadAllBytes(), type);
                    return Unwrap(response, responseId, sessionId, type);
                case "rpc_result":
                    if (responseId == sessionId)
                    {
                        // выполним распаковку ответа из Object
                        // rpc_result#f35c6d01 req_msg_id:long result:Object = RpcResult;
                        var raw = response.Get<byte[]>("result");

                        var combinator = new Combinator(raw, type);
                        return Unwrap(combinator, responseId, sessionId, type);
                    }
                    Trace.TraceWarning("Unexpected session id: {0}. Actual: {1}. Combinator: {2}", responseId, sessionId, response);
                    return null;
                default:
                    return response;
            }
        }

        private IEnumerable<SessionContainer> ExtractCombinators(TcpTransport item)
        {
            var results = new List<SessionContainer>();

            // снимем шифрование               
            var em = new EncryptedMessage(_settings.AuthKey, item.Payload);

            // если контейнер - вскроем его
            int crc32 = em.GetConstructorCrc();

            // Если контейнер
            // Простой контейнер содержит несколько сообщений следующим образом:
            // msg_container#73f1f8dc messages:vector message = MessageContainer;
            // Здесь message обозначает любое сообщение вместе с длиной и с msg_id:
            // message msg_id:long seqno:int bytes:int body:Object = Message;
            if (crc32 == 0x73f1f8dc)
            {
                // Распарсим все входные ответы
                using (var data = new MemoryStream(em.Data.MessageData))
                using (var br = new BinaryReader(data))
                {
                    br.ReadInt32(); // конструктор
                    int count = br.ReadInt32(); // количество сообщений внутри

                    for (int i = 0; i < count; i++)
                    {
                        br.ReadInt64();
                        br.ReadInt32();
                        int msgLength = br.ReadInt32();
                        byte[] msg = br.ReadBytes(msgLength);

                        // Внутри конструктора ожидаются следующие системные типы
                        //rpc_result#f35c6d01 req_msg_id:long result:Object = RpcResult;
                        //rpc_error#2144ca19 error_code:int error_message:string = RpcError;
                        //gzip_packed#3072cfa1 packed_data:string = Object;
                        // msgs_ack#62d6b459 msg_ids:Vector long = MsgsAck;

                        // Сформируем инстанс комбинатора и внесем его в очередь
                        var temp = new SessionContainer(em.Data.SessionId, msg);
                        results.Add(temp);
                    }
                }
            }
            else // простое сообщение
            {
                results.Add(new SessionContainer(em.Data.SessionId, em.Data.MessageData));
            }
            return results;
        }

        private void ProcessUpdates(Combinator combinator)
        {
            switch (combinator.Name)
            {
                // Обновлений накопилось слишком много, необходимо выполнить updates.getDifference
                case "updatesTooLong":
                    UpdateDifference();
                    break;
                default:
                    _updatingState.Update(combinator);
                    Updates(combinator);
                    break;
            }
        }

        private void UpdateDifference()
        {
            // updates.getDifference#5b36855a pts:int date:int = updates.Difference;
            RpcAnswer result = RpcCall(new Combinator("updates.getDifference", _updatingState.Pts, _updatingState.Date, _updatingState.Qts));

            // updates.differenceEmpty#5d75a138 date:int seq:int = updates.Difference;
            // updates.difference#8adb0077 new_messages:Vector<Message> other_updates:Vector<Update> chats:Vector<Chat> users:Vector<User> state:updates.State = updates.Difference;
            // updates.differenceSlice#c5e839b4 new_messages:Vector<Message> other_updates:Vector<Update> chats:Vector<Chat> users:Vector<User> intermediate_state:updates.State = updates.Difference;
            if (result.Success)
            {
                Combinator c = result.Combinator;
                switch (c.Name)
                {
                    case "updates.differenceEmpty":
                        _updatingState.Update(c);
                        return;
                    case "updates.difference":
                        _updatingState.Update(c.Get<Combinator>("state"));
                        break;
                    case "updates.differenceSlice":
                        _updatingState.Update(c.Get<Combinator>("intermediate_state"));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(c.Name);
                }

                Difference(c.Get<Combinator>("new_messages"));
                Difference(c.Get<Combinator>("other_updates"));
                Difference(c.Get<Combinator>("chats"));
                Difference(c.Get<Combinator>("users"));
            }
            else
                throw new Exception(result.Error.ToString());
        }

        private Settings LoadSettings()
        {
            byte[] authKey;
            long serverSalt;
            if (_persist.TryGet(_phone, out authKey, out serverSalt))
                return new Settings { AuthKey = authKey, NonceNewNonceXor = serverSalt };
            return new Settings();
        }

        private void SaveSettings(Settings settings)
        {
            _persist.Save(_phone, settings.AuthKey, settings.NonceNewNonceXor);
        }

        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private class State
        {
            public int Pts { get; private set; }
            public int Qts { get; private set; }
            public int Date { get; private set; }
            public int Seq { get; private set; }

            public void Update(Combinator c)
            {
                int value;
                if (c.TryGet("pts", out value))
                    Pts = value;
                if (c.TryGet("qts", out value))
                    Qts = value;
                if (c.TryGet("date", out value))
                    Date = value;
                if (c.TryGet("seq", out value))
                    Seq = value;
            }
        }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
        // ReSharper restore MemberCanBePrivate.Local
    }
}