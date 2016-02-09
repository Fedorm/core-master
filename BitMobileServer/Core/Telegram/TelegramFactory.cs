using System;
using System.Collections;
using System.Collections.Generic;

namespace Telegram
{
    public class TelegramFactory
    {
        private readonly ITelegramPersist _persist;

        public TelegramFactory(ITelegramPersist persist)
        {
            _persist = persist;
        }

        private static readonly Dictionary<string, TelegramClient> Clients = new Dictionary<string, TelegramClient>();
        
        public TelegramClient Client(string phone)
        {
            TelegramClient client;
            if (!Clients.TryGetValue(phone, out client))
            {
                client = new TelegramClient(phone, _persist);
                Clients.Add(phone, client);
            }

            return client;
        }

        public object Comb(string name)
        {
            return new Combinator(name, PrepareArgs(null));
        }

        public object Comb(string name, object args)
        {
            return new Combinator(name, PrepareArgs(args));
        }

        public object Comb(string name, string type, object args)
        {
            return new Combinator(name, type, PrepareArgs(args));
        }

        public int GetRandom()
        {
            return new Random().Next(int.MaxValue);
        }
        
        internal static object[] PrepareArgs(object args)
        {
            if (args == null)
                return new object[0];

            var list = args as ArrayList;
            if (list != null)
                return list.ToArray();

            return new[] { args };
        }
    }
}
