using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Telegram.Authorize;

namespace Telegram.Schema
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public class ApiSchema
    {
        public ApiSchema()
        {
            constructors = new List<Constructor>();
            methods = new List<Method>();
        }

        public List<Constructor> constructors { get; private set; }

        public List<Method> methods { get; private set; }

        public void Load(Stream stream)
        {
            using (var file = new StreamReader(stream))
            {
                var serializer = new JsonSerializer();
                var schema = (ApiSchema)serializer.Deserialize(file, typeof(ApiSchema));

                constructors.AddRange(schema.constructors);
                methods.AddRange(schema.methods);
            }
        }

        internal SchemaCombinator FindCombinator(string name)
        {
            SchemaCombinator combinator = methods.Find(val => val.method == name);
            if (combinator == null)
            {
                combinator = constructors.Find(val => val.predicate == name);
                if (combinator == null)
                    throw new ArgumentOutOfRangeException(name);
            }

            return combinator;
        }

        internal SchemaCombinator FindCombinator(int id)
        {
            SchemaCombinator combinator = methods.Find(val => val.id == id);
            if (combinator == null)
            {
                combinator = constructors.Find(val => val.id == id);
                if (combinator == null)
                    throw new DecodeException("Combinator not found: " + id.ToString(CultureInfo.InvariantCulture));
            }

            return combinator;
        }

        internal Argument[] FindArguments(SchemaCombinator combinator)
        {
            return combinator.@params.Select(val => new Argument(val.name, val.type)).ToArray();
        }

        internal string[] FindTypes(SchemaCombinator combinator)
        {
            return combinator.@params.Select(val => val.type).ToArray();
        }

        internal int FindVectorId()
        {
            return constructors.Find(val => val.type == "Vector t").id;
        }

        internal class Argument
        {
            public Argument(string name, string type)
            {
                Name = name;
                Type = type;
            }

            public string Name { get; private set; }
            public string Type { get; private set; }
        }
    }
}