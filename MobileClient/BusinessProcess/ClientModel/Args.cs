using System.Collections.Generic;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
namespace BitMobile.BusinessProcess.ClientModel
{
    public class Args<TResult>
    {
        public Args(TResult result)
        {
            Result = result;
        }

        public TResult Result { get; private set; }
    }

    public class KeyValueArgs<TKey, TValue> : Args<TKey> where TKey : class
    {
        public KeyValueArgs(TKey key, IEnumerable<KeyValuePair<TKey, TValue>> values)
            : base(HandleKey(key))
        {
            foreach (var keyValuePair in values)
                if (keyValuePair.Key.Equals(key))
                {
                    Value = keyValuePair.Value;
                    break;
                }
        }

        public TKey Key { get { return Result; } }

        public TValue Value { get; private set; }

        private static TKey HandleKey(TKey key)
        {
            if (key == null)
                return null;

            return key.Equals(Dialog.NullChooseKey) ? null : key;
        }
    }
}