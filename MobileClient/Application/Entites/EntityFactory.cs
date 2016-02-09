using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using BitMobile.Common.DbEngine;
using BitMobile.Common.Entites;
using BitMobile.Common.ValueStack;

namespace BitMobile.Application.Entites
{
    public static class EntityFactory
    {
        private const string IdFieldName = "Id";

        private static EntityType[] _knownTypes;

        public static object CreateEntity(string name)
        {
            String[] arr = name.Split('.');
            if (arr.Length == 2)
            {
                EntityType t = FindType(name);
                return CreateInstance(t);
            }
            throw new Exception(String.Format("Cant create instance of type '{0}'", name));
        }

        // todo: убрать этот пиздец
        public static Func<IEntityType, IEntity> CreateInstance { get; set; }

        public static Func<string, Guid, IDbRef> DbRefFactory { get; set; }

        public static Func<IDictionary<string, object>> CustomDictionaryFactory { get; set; }

        public static EntityType FindType(string input)
        {
            input = input.Replace('.', '_');
            foreach (EntityType e in _knownTypes)
                if (e.TableName == input)
                    return e;
            throw new Exception(String.Format("Type '{0}' is not found.", input));
        }

        public static EntityType[] RegisterKnownTypes(XmlDocument doc)
        {
            var result = new List<EntityType>();
            result.Add(GetAdmin());
            result.AddRange(GetResources());

            XmlNodeList nodes = doc.DocumentElement.SelectNodes("//Entities/Entity");
            foreach (XmlNode node in nodes)
            {
                EntityType e = CreateEntityType(node);
                result.Add(e);

                XmlNodeList tabularSectionNodes = node.SelectNodes("TabularSections/TabularSection");

                foreach (XmlNode tabularSectionNode in tabularSectionNodes)
                    result.Add(CreateTabularSection(tabularSectionNode, e.Schema, e.Name));

            }
            _knownTypes = result.ToArray();
            return _knownTypes;
        }

        public static IDictionary<string, object> GetConstants(XmlDocument doc)
        {
            var result = CustomDictionaryFactory();
            XmlNodeList nodes = doc.DocumentElement.SelectNodes("//Configuration/Constants/Entity");
            foreach (XmlNode node in nodes)
            {
                string entityName = node.Attributes["Name"].Value;
                var constant = CustomDictionaryFactory();
                result.Add(entityName.Split('.')[1], constant);

                XmlNodeList rows = node.SelectNodes("Row");
                foreach (XmlNode row in rows)
                {
                    string name = row.Attributes["Name"].Value.Replace(' ', '_');
                    var id = new Guid(row.Attributes["Id"].Value);
                    IDbRef dbRef = DbRefFactory(entityName.Replace('.', '_'), id);
                    constant.Add(name, dbRef);
                }
            }

            return result;
        }

        private static EntityType CreateEntityType(XmlNode node)
        {
            string schema = node.Attributes["Schema"].Value;
            string name = node.Attributes["Name"].Value;

            XmlNodeList fieldNodes = node.SelectNodes("Fields/Field");
            var fields = new List<IEntityField>(fieldNodes.Count);
            for (int i = 0; i < fieldNodes.Count; i++)
                fields.Add(CreateEntityField(fieldNodes[i], i, ToTableName(schema, name)));

            return new EntityType(schema, name, fields, IdFieldName);
        }

        private static EntityType CreateTabularSection(XmlNode node, string schema, string parentName)
        {
            string name = parentName + "_" + node.Attributes["Name"].Value;

            XmlNodeList fieldNodes = node.ChildNodes;
            var fields = new List<IEntityField>(fieldNodes.Count);
            for (int i = 0; i < fieldNodes.Count; i++)
                fields.Add(CreateEntityField(fieldNodes[i], i, ToTableName(schema, name), ToTableName(schema, parentName)));

            return new EntityType(schema, name, fields, IdFieldName);
        }

        private static EntityField CreateEntityField(XmlNode node, int index, string entityName, string refName = null)
        {
            string name = node.Attributes["Name"].Value;
            string typeName = node.Attributes["Type"].Value;
            bool keyField = false;
            bool allowNull = false;
            string dbRefTable = null;

            foreach (XmlAttribute attribute in node.Attributes)
                switch (attribute.Name)
                {
                    case "KeyField":
                        keyField = bool.Parse(attribute.Value);
                        break;
                    case "AllowNull":
                        allowNull = bool.Parse(attribute.Value);
                        break;
                }

            Type type;
            if (name == "Id" || name == "Ref")
            {
                if (typeName.ToUpper() == "GUID")
                {
                    type = typeof(IDbRef);
                    if (name == "Id")
                        dbRefTable = entityName;
                    else if (name == "Ref")
                        dbRefTable = refName;
                }
                else
                    throw new Exception("Invalid primary key type: " + typeName);
            }
            else
                type = ParseType(typeName, allowNull && !keyField, out dbRefTable);

            if (type == typeof(IDbRef) && dbRefTable == null)
                throw new Exception();

            return new EntityField(name, type, keyField, allowNull, index, dbRefTable);
        }

        private static Type ParseType(string type, bool nullable, out string dbRefTable)
        {
            dbRefTable = null;
            string upper = type.ToUpper();

            switch (upper)
            {
                case "GUID":
                    return nullable ? typeof(Guid?) : typeof(Guid);
                case "BOOLEAN":
                    return nullable ? typeof(Boolean?) : typeof(Boolean);
                case "INTEGER":
                    return nullable ? typeof(Int32?) : typeof(Int32);
                case "STRING":
                    return typeof(String);
                case "BLOB":
                    return typeof(byte[]);
                case "DATETIME":
                case "DATETIME2":
                    return nullable ? typeof(DateTime?) : typeof(DateTime);
                case "DECIMAL":
                    return nullable ? typeof(Decimal?) : typeof(Decimal);
                default:
                    if (type.Contains('.'))
                    {
                        if (type.Split('.').Count() > 2)
                            throw new Exception();
                        dbRefTable = type.Replace('.', '_');
                        return typeof(IDbRef);
                    }
                    throw new Exception(String.Format("unknown data type {0}", type));
            }
        }

        private static IEnumerable<EntityType> GetResources()
        {
            var entities = new[]
            {
                "Configuration",
                "BusinessProcess",
                "Image",
                "Screen",
                "Script",
                "Style",
                "Translation"
            };

            const string schema = "resource";
            var result = new List<EntityType>();
            foreach (String entity in entities)
            {
                var fields = new List<IEntityField>
                {
                    new EntityField(IdFieldName, typeof(IDbRef), true, true, 0, ToTableName(schema, entity)),
                    new EntityField("Name", typeof(string), false, false, 1 ),
                    new EntityField("Data", typeof(string), false, false, 2 ),
                    new EntityField("Parent", typeof(string), false, false, 3 )
                };

                var e = new EntityType(schema, entity, fields, IdFieldName);

                result.Add(e);
            }
            return result;
        }

        private static EntityType GetAdmin()
        {
            var fields = new List<IEntityField>
                {
                    new EntityField(IdFieldName, typeof(IDbRef), true, true, 0, "admin_Entity" ),
                    new EntityField("Name", typeof(string), false, false, 1 ),
                    new EntityField("Schema", typeof(string), false, false, 2 ),
                    new EntityField("ShortName", typeof(string), false, false, 3 )
                };
            return new EntityType("admin", "Entity", fields, IdFieldName);
        }

        private static string ToTableName(string schema, string entity)
        {
            return string.Format("{0}_{1}", schema, entity);
        }

    }
}