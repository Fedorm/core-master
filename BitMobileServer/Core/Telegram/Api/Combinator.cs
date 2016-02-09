using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BitMobile.ValueStack;
using Telegram.Math;
using Telegram.Schema;
using Telegram.Translation;

namespace Telegram
{
    class Combinator : IEnumerable<object>, IIndexedProperty
    {
        private const string VectorName = "Vector";

        private static ApiSchema _schema;
        private static IFormatter<byte[]> _formatter;

        private KeyValuePair<string, object>[] _arguments;
        private SchemaCombinator _descriptor;
        private string _genericType;
        private object[] _values;

        public Combinator(byte[] raw, string expectedType)
        {
            int offset = 0;
            Decompile(raw, ref offset, expectedType);
        }

        public Combinator(string name, params object[] args)
        {
            _descriptor = _schema.FindCombinator(name);
            _genericType = null;
            FillArguments(args);
        }

        public Combinator(string name, string type, params object[] args)
        {
            _descriptor = _schema.FindCombinator(name);
            _genericType = type;
            if (IsVector)
                _values = args;
            else
                FillArguments(args);
        }

        protected Combinator(Combinator combinator)
        {
            _descriptor = combinator._descriptor;

            _arguments = new KeyValuePair<string, object>[combinator._arguments.Length];
            Array.Copy(combinator._arguments, _arguments, combinator._arguments.Length);
        }

        private Combinator(Lexer lexer)
        {
            Compile(lexer);
        }

        private Combinator(byte[] raw, ref int offset, string expectedType)
        {
            Decompile(raw, ref offset, expectedType);
        }

        public string Name
        {
            get { return Descriptor.GetName(); }
        }

        public SchemaCombinator Descriptor
        {
            get { return _descriptor; }
        }

        public bool IsVector
        {
            get { return Descriptor.id == _schema.FindVectorId(); }
        }

        public object this[string name]
        {
            get { return _arguments.First(val => val.Key == name).Value; }
        }

        public object this[int index]
        {
            get { return _values[index]; }
        }

        public IEnumerator<object> GetEnumerator()
        {
            if (_values != null)
                foreach (object t in _values)
                    yield return t;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Compile

        private void Compile(Lexer lexer)
        {
            lexer.Next("\\(");
            string name = lexer.Next("(\\w|\\.)*").Trim();

            _descriptor = _schema.FindCombinator(name);

            ApiSchema.Argument[] args = _schema.FindArguments(Descriptor);
            _arguments = new KeyValuePair<string, object>[args.Length];
            for (int i = 0; i < args.Length; i++)
                _arguments[i] = new KeyValuePair<string, object>(args[i].Name, Argument(lexer, args[i].Type));

            lexer.Next(@"\)");
        }

        private object Argument(Lexer lexer, string type)
        {
            switch (type)
            {
                case "int":
                    string integer = lexer.Next("\\d+").Trim();
                    return int.Parse(integer);
                case "long":
                    string longint = lexer.Next("\\d+").Trim();
                    return long.Parse(longint);
                case "int128":
                    string int128 = lexer.Next("\\d+").Trim();
                    return BigInteger.Parse(int128);
                case "int256":
                    string int256 = lexer.Next("\\d+").Trim();
                    return BigInteger.Parse(int256);
                case "string":
                    return lexer.Next("\".*\"").Trim().Trim('\"');
                case "bytes":
                case "Object":
                    return Encoding.UTF8.GetBytes(lexer.Next("\\S*"));
                case "double":
                    throw new NotImplementedException();
                default:
                    if (type.StartsWith(VectorName))
                        throw new NotImplementedException();
                    return new Combinator(lexer);
            }
        }

        private IList<object> Vector(Lexer lexer, string type)
        {
            lexer.Next("\\(");
            lexer.Next("\\[");

            var result = new List<object>();
            int i = 0;
            while (!lexer.Look("\\]"))
            {
                if (i > 0)
                    lexer.Next(",");
                result.Add(Argument(lexer, type));
                i++;
            }

            lexer.Next("\\]");
            lexer.Next("\\)");

            return result;
        }

        #endregion

        #region Decompile

        private void Decompile(byte[] raw, ref int offset, string expectedType)
        {
            int id = _formatter.ReadInt32(raw, ref offset);
            _descriptor = _schema.FindCombinator(id);
            _genericType = VectorType(expectedType);

            if (IsVector)
            {
                int length = _formatter.ReadInt32(raw, ref offset);
                _values = new object[length];

                for (int i = 0; i < length; i++)
                    _values[i] = Argument(raw, ref offset, _genericType);
            }
            else
            {
                ApiSchema.Argument[] args = _schema.FindArguments(Descriptor);
                _arguments = new KeyValuePair<string, object>[args.Length];
                for (int i = 0; i < args.Length; i++)
                    _arguments[i] = new KeyValuePair<string, object>(args[i].Name,
                        Argument(raw, ref offset, args[i].Type));
            }
        }

        private object Argument(byte[] raw, ref int offset, string type)
        {
            if (IsBareType(type))
                throw new NotImplementedException();

            switch (type)
            {
                case "int":
                    return _formatter.ReadInt32(raw, ref offset);
                case "long":
                    return _formatter.ReadInt64(raw, ref offset);
                case "int128":
                    return _formatter.ReadInt128(raw, ref offset);
                case "int256":
                    return _formatter.ReadInt256(raw, ref offset);
                case "string":
                    return _formatter.ReadString(raw, ref offset);
                case "bytes":
                    return _formatter.ReadBytes(raw, ref offset);
                case "Object":
                    return _formatter.ReadToEnd(raw, ref offset);
                case "double":
                    throw new NotImplementedException();
                default:
                    return new Combinator(raw, ref offset, type);
            }
        }

        #endregion

        #region Serialization

        private byte[] SerializeValue(string type, object value)
        {
            bool bareType = IsBareType(type);
            if (bareType)
                type = type.Trim('%');

            switch (type)
            {
                case "int":
                    return _formatter.FromInt32(Convert.ToInt32(value));
                case "long":
                    return _formatter.FromInt64(Convert.ToInt64(value));
                case "int128":
                    return _formatter.FromInt128((BigInteger)value);
                case "int256":
                    return _formatter.FromInt256((BigInteger)value);
                case "string":
                    return _formatter.FromString((string)value);
                case "bytes":
                case "Object":
                    return _formatter.FromBytes((byte[])value);
                case "double":
                    throw new NotImplementedException();
                default:
                    return ((Combinator)value).Serialize(bareType);
            }
        }

        #endregion

        public static void Setup(ApiSchema schema, IFormatter<byte[]> formatter)
        {
            _schema = schema;
            _formatter = formatter;
        }

        public static Combinator Parse(string input)
        {
            return new Combinator(new Lexer(input));
        }

        public bool TryGet<T>(string name, out T value)
        {
            if (_arguments != null && _arguments.Count(val => val.Key == name) > 0)
            {
                value = Get<T>(name);
                return true;
            }
            value = default(T);
            return false;
        }

        public T Get<T>(string name)
        {
            return (T)this[name];
        }

        public T Get<T>(int index)
        {
            return (T)this[index];
        }

        public byte[] Serialize(bool bareType = false)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                if (!bareType)
                    writer.Write(Descriptor.id);

                if (IsVector)
                {
                    writer.Write(_values.Length);
                    foreach (object value in _values)
                        writer.Write(SerializeValue(_genericType, value));
                }
                else
                {
                    ApiSchema.Argument[] arguments = _schema.FindArguments(Descriptor);
                    foreach (ApiSchema.Argument a in arguments)
                    {
                        byte[] b = SerializeValue(a.Type, this[a.Name]);
                        writer.Write(b);
                    }
                }
                return stream.ToArray();
            }
        }
        
        #region IIndexedProperty

        public object GetValue(string propertyName)
        {
            return this[propertyName];
        }

        public bool HasProperty(string propertyName)
        {
            return _arguments.Count(val => val.Key == propertyName) > 0;
        }
        #endregion

        public override string ToString()
        {
            var builder = new StringBuilder("(");
            builder.Append(Descriptor.GetName()).Append(' ');

            if (IsVector)
            {
                builder.Append('[');
                for (int i = 0; i < _values.Length; i++)
                {
                    if (i > 0)
                        builder.Append(", ");
                    builder.Append(ArgumentToString(_values[i]));
                }
                builder.Append(']');
            }
            else
                for (int i = 0; i < _arguments.Length; i++)
                {
                    if (i > 0)
                        builder.Append(' ');
                    builder.Append(ArgumentToString(_arguments[i].Value));
                }

            builder.Append(')');
            return builder.ToString();
        }

        private void FillArguments(object[] args)
        {
            ApiSchema.Argument[] arguments = _schema.FindArguments(Descriptor);
            _arguments = new KeyValuePair<string, object>[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                string argName = arguments[i].Name;
                _arguments[i] = new KeyValuePair<string, object>(argName, args[i]);
            }
        }

        private static string VectorType(string type)
        {
            return type != null ? type.Replace(VectorName, "").Trim('<', '>') : null;
        }

        private static bool IsBareType(string type)
        {
            return type.StartsWith("%");
        }

        private static string ArgumentToString(object value)
        {
            var raw = value as byte[];
            if (raw != null)
                return Encoding.UTF8.GetString(raw);

            var str = value as string;
            if (str != null)
                return string.Format("\"{0}\"", str);

            var vector = value as IList<object>;
            if (vector != null)
            {
                throw new NotImplementedException();
            }

            return value.ToString();
        }
    }
}