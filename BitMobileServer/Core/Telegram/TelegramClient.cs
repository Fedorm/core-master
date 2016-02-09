using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Telegram.Api;
using Telegram.Schema;
using Telegram.Service;

namespace Telegram
{
    public class TelegramClient
    {
        private const string Address = "149.154.167.50";
        private const int Port = 443;
        private const int ApiId = 15066;
        private const string ApiHash = "9bedd7f3356cf9d8b0b0da28341cbd6c";
        private const string ConfigDevice = "bitmobile_server";
        private const string ConfigSystem = "windows";
        private const string ConfigApplication = "1";
        private const string LangCode = "RU";
        private const int ApiLayer = 23;

        private readonly List<Combinator> _updates = new List<Combinator>();
        private readonly List<Combinator> _differences = new List<Combinator>();
        private readonly Provider _provider;
        private readonly string _phoneNumber;
        private string _smsHash;
        private bool _phoneRegistered;

        public TelegramClient(string phoneNumber, ITelegramPersist persist)
        {
            _phoneNumber = phoneNumber;

            try
            {
                var schema = new ApiSchema();

                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(Shemas.Schema)))
                    schema.Load(ms);
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(Shemas.SchemaMtProto)))
                    schema.Load(ms);
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(Shemas.SchemaEndToEnd)))
                    schema.Load(ms);

                _provider = new Provider(schema, Address, Port, phoneNumber.Replace("+", ""), persist);
                _provider.Updates += OnUpdate;
                _provider.Difference += OnDifference;
            }
            catch (TlException)
            {
                throw;
            }
            catch (Exception e)
            {
                e = e is AggregateException ? e.InnerException : e;
                throw new TlException(e);
            }
        }

        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnusedMember.Global

        public bool Authorized
        {
            get
            {
                try
                {
                    return _provider.Authorized;
                }
                catch (Exception e)
                {
                    e = e is AggregateException ? e.InnerException : e;
                    throw new TlException(e);
                }
            }
        }

        
        public bool Connect()
        {
            if (!_provider.CheckAndGenerateAuth())
                throw new TlException(new Exception("Auth key not created"));

            try
            {
                bool authorized = _provider
                    .InitConnection(ApiId, ConfigDevice, ConfigSystem, ConfigApplication, LangCode, ApiLayer);

                if (!authorized)
                {
                    Combinator result = RpcCall("auth.sendCode", _phoneNumber, 0, ApiId, ApiHash, LangCode);
                    _smsHash = result.Get<string>("phone_code_hash");
                    _phoneRegistered = (result.Get<Combinator>("phone_registered")).Name == "boolTrue";
                }

                return authorized;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                throw new TlException(e);
            }
        }

        public void Authorize(string smsCode)
        {
            if (string.IsNullOrEmpty(_smsHash))
                throw new TlException(new Exception("Empty sms hash"));

            try
            {
                if (_phoneRegistered)
                    RpcCall("auth.signIn", _phoneNumber, _smsHash, smsCode);
                else
                    RpcCall("auth.signUp", _phoneNumber, _smsHash, smsCode, "Bit", "Mobile");

                _provider.Authorized = true;
                _provider.UpdateState();
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                throw new TlException(e);
            }
        }

        public object Rpc(string name, object args)
        {
            if (!Authorized)
                throw new TlException(new Exception("Client not authorized"));

            return RpcCall(name, TelegramFactory.PrepareArgs(args));
        }

        public void Update()
        {
            if (!Authorized)
                throw new TlException(new Exception("Authorization error"));
            _provider.Ping();
        }

        public IEnumerable<object> GetUpdates()
        {
            Update();
            var result = new List<object>(_updates);
            _updates.Clear();
            return result;
        }

        public IEnumerable<object> GetDifferences()
        {
            Update();
            var result = new List<object>(_differences);
            _differences.Clear();
            return result;
        }

        public void SendMessage(string phone, string message)
        {
            Combinator result = RpcCall("auth.checkPhone", phone);
            if (result.Get<Combinator>("phone_registered").Name == "boolFalse")
                throw new TlException(new Exception("Phone not registered: " + phone));

            if (result.Get<Combinator>("phone_invited").Name == "boolFalse")
            {
                do
                {
                    result = RpcCall("auth.sendInvites"
                        , new Combinator("vector", "string", phone)
                        , "Invite");
                } while (result.Name == "boolFalse");

                RpcCall("contacts.importContacts"
                    , new Combinator("vector", "InputContact"
                        , new Combinator("inputPhoneContact", new Random().Next(), phone, "c_" + phone, "c_" + phone))
                    , new Combinator("boolTrue"));
            }

            result = RpcCall("contacts.getContacts", new Random().Next(int.MaxValue).ToString(CultureInfo.InvariantCulture));
            var users = result.Get<Combinator>("users");

            if (users.Count(val => ((Combinator)val).Get<string>("phone") == phone) > 0)
            {
                var userId = ((Combinator)users.First(val => ((Combinator)val).Get<string>("phone") == phone)).Get<int>("id");
                RpcCall("messages.sendMessage", new Combinator("inputPeerContact", userId), message, new Random().Next(int.MaxValue));
            }
            else
                throw new TlException(new Exception("Contact not exists: " + phone));
        }

        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore UnusedMember.Global

        private Combinator RpcCall(string name, params object[] parameters)
        {
            RpcAnswer answer;
            try
            {
                var combinator = new Combinator(name, parameters);
                answer = _provider.PerformRpcCall(combinator);
            }
            catch (Exception e)
            {
                e = e is AggregateException ? e.InnerException : e;
                throw new TlException(e);
            }

            if (!answer.Success)
                throw new TlException(new Exception(answer.Error.ToString()));
            return answer.Combinator;
        }

        private void OnUpdate(Combinator obj)
        {
            _updates.Add(obj);
        }

        private void OnDifference(Combinator obj)
        {
            _differences.Add(obj);
        }
    }
}
