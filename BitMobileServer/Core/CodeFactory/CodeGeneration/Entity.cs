using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeFactory
{
    public class Config
    {
        private List<Entity> entities;
        private Dictionary<String, List<Constant>> constants;
        private Dictionary<String, Parameter> globalParameters;
        private String solutionName;
        private ConfigVersion configVersion;
        private List<KeyInfo> clusteredPrimaryKeys;
        private List<KeyInfo> nonClusteredPrimaryKeys;

        public ConfigVersion ConfigVersion
        {
            get { return configVersion; }
        }

        public Config(ConfigVersion version, String solutionName, List<Entity> entities, Dictionary<String, Parameter> globalParameters, Dictionary<String, List<Constant>> constants)
        {
            this.configVersion = version;
            this.solutionName = solutionName;
            this.entities = entities;
            this.constants = constants;
            this.globalParameters = globalParameters;

            foreach (Entity entity in this.Entities)
            {
                entity.BuildSyncFilter(this);
            }
        }

        public void BuildNonClusteredPrimaryKeys(String connectionString)
        {
            nonClusteredPrimaryKeys = KeysPatcher.GetNonClusteredKeys(this, connectionString);
        }

        public void BuildClusteredPrimaryKeys(String connectionString)
        {
            clusteredPrimaryKeys = KeysPatcher.GetClusteredKeys(this, connectionString);
        }

        public List<KeyInfo> ClusteredPrimaryKeys
        {
            get
            {
                return clusteredPrimaryKeys;
            }
        }

        public List<KeyInfo> NonClusteredPrimaryKeys
        {
            get
            {
                return nonClusteredPrimaryKeys;
            }
        }

        public List<Entity> Entities
        {
            get
            {
                return entities;
            }
        }

        public Dictionary<String, List<Constant>> Constants
        {
            get
            {
                return constants;
            }
        }

        public Dictionary<String, Parameter> GlobalParameters
        {
            get
            {
                return globalParameters;
            }
        }

        public Dictionary<String, List<Entity>> EntitiesBySchema
        {
            get
            {
                Dictionary<String, List<Entity>> result = new Dictionary<string, List<Entity>>();
                foreach (var e in entities)
                {
                    if (!result.ContainsKey(e.Schema))
                        result.Add(e.Schema, new List<Entity>());
                    result[e.Schema].Add(e);
                }
                return result;
            }
        }

        public String Scope
        {
            get
            {
                return "DefaultScope";
            }            
        }

        public String ServerScope
        {
            get
            {
                return "DefaultScope";
            }
        }

        public String DatabaseName
        {
            get
            {
                return String.Format("{0}", solutionName);
            }
        }

        public String SolutionName
        {
            get
            {
                return solutionName;
            }
        }

    }

    public class ConfigVersion
    {
        private String name;

        public String Name
        {
            get { return name; }
            set { name = value; }
        }
        private String version;

        public String Version
        {
            get { return version; }
            set { version = value; }
        }

        public String VersionMasked
        {
            get
            {
                String[] arr = version.Split('.');
                String result = "";
                for (int i = 0; i < arr.Length - 1; i++)
                    result = result + arr[i] + ".";
                return result + "%";
            }
        }

    }


    public class Entity
    {
        private Entity parent;
        private String schema;
        private String name;
        private bool syncUpload;
        private bool syncDownload;
        private String syncFilter="";
        private String syncFilter2="";
        private bool hasDeletedField = false;

        private Dictionary<String, String> filterColumns;
        private Dictionary<String, Parameter> filterParameters;

        public String Schema
        {
            get { return schema; }
            set { this.schema = value; }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public bool SyncUpload
        {
            get { return syncUpload; }
            set { syncUpload = value; }
        }

        public bool SyncDownload
        {
            get { return syncDownload; }
            set { syncDownload = value; }
        }

        public String SyncFilter
        {
            get { return syncFilter; }
            set 
            { 
                syncFilter = value; 
            }
        }

        public String SyncFilterFake
        {
            get 
            {
                return String.IsNullOrEmpty(syncFilter) ? "" : "1=1"; 
            }
        }

        public String SyncFilterPatched
        {
            get
            {
                //return syncFilter.Replace("[side].", "[parent].");
                String f1 = parent.syncFilter.Replace("[side].", "[parent].");
                String f2 = this.syncFilter.Replace("[side].", "[base].");

                String result = f1;
                if (!String.IsNullOrEmpty(result) && !String.IsNullOrEmpty(f2))
                    result = result + " AND ";
                result = result + f2;

                return result;
            }
        }

        public bool SyncFilterIsComplicated
        {
            get
            {
                if (String.IsNullOrEmpty(syncFilter2))
                    return false;
                else
                    return new Regex(@"\s+in\s*\(", RegexOptions.IgnoreCase).IsMatch(syncFilter2);
            }
        }

        public bool SyncFilterIsComplicatedEx
        {
            get
            {
                if (this.SyncFilterIsComplicated)
                    return true;
                foreach (Entity ts in this.tabularSections)
                {
                    if (ts.SyncFilterIsComplicated)
                        return true;
                }
                return false;
            }
        }

        public String SyncFilter2
        {
            get
            {
                return syncFilter2;
            }
        }

        public String SyncFilterColumn
        {
            get
            {
                return filterColumns.Keys.First<String>();
            }
        }

        /*
        public String SyncFilter2Ref
        {
            get
            {
                return syncFilter2.ToLower().Replace("t.id","Ref.[Id]").Replace("t.id","Ref.[Id]"
            }

            return syncFilter2.Lowe  Replace("t.[Id]", "[side].");
                            syncFilter = syncFilter.Replace("t.", "[side].");
                syncFilter = syncFilter.Replace("T.", "[side].");

        }
        */
        private FieldList fields;

        public FieldList Fields
        {
            get { return fields; }
        }

        public IEnumerable<Field> FieldsExceptKey
        {
            get
            {
                foreach (Field f in fields)
                {
                    if (!f.KeyField)
                        yield return f;
                }
            }
        }

        private List<TabularSection> tabularSections;

        public List<TabularSection> TabularSections
        {
            get { return tabularSections; }
        }

        public String KeyField
        {
            get
            {
                foreach (var item in fields)
                {
                    if (item.KeyField)
                        return item.Name;
                }
                throw new Exception(String.Format("Key field is not found for entity:{0}", this.name));
            }
        }

        public bool LastField(String fieldName)
        {
            return this.Fields[this.Fields.Count - 1].Name.ToLower().Equals(fieldName.ToLower());
        }
        
        public bool HasDeletedField()
        {
            return hasDeletedField && (schema.ToLower() != "enum");
        }
        
        public List<String> SyncFilterColumns
        {
            get
            {
                return filterColumns.Values.ToList();
            }
        }

        public Field SyncFilterField
        {
            get
            {
                foreach (Field f in fields)
                {
                    if(f.Name.ToLower().Equals(SyncFilterColumn.ToLower()))
                        return f;
                }
                throw new Exception("Unable to recognize SyncFilterField");
            }
        }

        public List<Parameter> SyncFilterParameters
        {
            get
            {
                if (parent == null)
                    return filterParameters.Values.ToList();
                else
                    return parent.SyncFilterParameters.Union(filterParameters.Values.ToList()).ToList();
            }
        }

        public Entity(Entity parent, bool hasDeletedField = false)
        {
            this.parent = parent;
            this.fields = new FieldList(this);
            this.tabularSections = new List<TabularSection>();
            this.hasDeletedField = hasDeletedField;
        }

        public void BuildSyncFilter(Config config)
        {
            filterColumns = new Dictionary<String, String>();
            filterParameters = new Dictionary<String, Parameter>();
            if (!String.IsNullOrEmpty(syncFilter))
            {
                Regex r = new System.Text.RegularExpressions.Regex(@"(t\.\[*(?<field>\w+)\]*)|(@(?<parameter>\w+))", RegexOptions.IgnoreCase);
                var matches = r.Matches(syncFilter);
                foreach (Match m in matches)
                {
                    foreach (Capture c in m.Groups["field"].Captures)
                    {
                        String part = c.Value;
                        Field field = null;
                        foreach (Field f in fields)
                        {
                            if (f.Name.Equals(part))
                            {
                                field = f;
                                break;
                            }
                        }
                        //if (field == null)
                        //    throw new Exception("There is no field '" + part + "' in entity" + this.Name);
                        if (field != null)
                        {
                            if (!filterColumns.ContainsKey(field.Name))
                                filterColumns.Add(field.Name, field.Name);
                        }
                    }

                    foreach (Capture c in m.Groups["parameter"].Captures)
                    {
                        String part = c.Value;
                        if (!config.GlobalParameters.ContainsKey(part))
                            throw new Exception("There is no global parameter " + part);
                        if (!filterParameters.ContainsKey(part))
                            filterParameters.Add(part, config.GlobalParameters[part]);
                    }
                }

                syncFilter2 = syncFilter;
                syncFilter = syncFilter.Replace("t.", "[side].");
                syncFilter = syncFilter.Replace("T.", "[side].");
            }

            foreach (var ts in this.TabularSections)
            {
                ts.BuildSyncFilter(config);
                if (ts.SyncFilterIsComplicated)
                {
                    if (ts.SyncFilterColumn.ToLower().Equals("id"))
                        throw new Exception(String.Format("{0}.{1}_{2} - tabular secton filter must not reference Id column", this.Schema, this.name, ts.name));
                }
            }
        }

        public void SortFields()
        {
            List<Field> fs = new List<Field>();
            String order = "Id,Ref,LineNumber";
            String[] arr = order.Split(',');

            foreach (String s in arr)
            {
                for (int j = 0; j < this.Fields.Count; j++)
                {
                    Field f = this.Fields[j];
                    if (f.Name.ToLower().Equals(s.ToLower()))
                    {
                        Fields.Remove(f);
                        fs.Insert(0, f);
                        break;
                    }
                }
            }

            foreach (Field f in fs)
            {
                Fields.Insert(0, f);
            }
        }
    }

    public class TabularSection : Entity
    {
        private String key;
        public String Key 
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
            }
        }

        public bool HasTabularSectionKey
        {
            get
            {
                return !String.IsNullOrEmpty(key);
            }
        }


        public IEnumerable<Field> TabularSectionKeys
        {
            get
            {
                if (!String.IsNullOrEmpty(key))
                {
                    foreach (String s in key.Split(','))
                    {
                        String keyName = s.Trim();
                        foreach (Field f in Fields)
                        {
                            if (f.Name.ToLower().Equals(keyName.ToLower()))
                                yield return f;
                        }
                    }
                }
            }
        }

        public bool LastTabularSectionKey(String keyName)
        {
            String[] arr = key.Split(',');
            return arr[arr.Length - 1].ToLower().Trim().Equals(keyName.ToLower());
        }


        public IEnumerable<Field> TabularFieldsExceptKeys
        {
            get
            {
                Dictionary<String, Field> keys = new Dictionary<string, Field>();
                foreach (Field f in TabularSectionKeys)
                {
                    keys.Add(f.Name.ToLower(), f);
                }

                foreach (Field f in Fields)
                {
                    if (!f.KeyField && !f.Name.ToLower().Equals("ref") && !keys.ContainsKey(f.Name.ToLower()))
                        yield return f;
                }
            }
        }

        public IEnumerable<Field> TabularFieldsExceptRefAndKey
        {
            get
            {
                foreach (Field f in Fields)
                {
                    if (!f.KeyField && !f.Name.ToLower().Equals("ref"))
                        yield return f;
                }
            }
        }

        public String SyncFilterSimple
        {
            get
            {
                if(String.IsNullOrEmpty(SyncFilter2))
                    return "1=1";
                else
                    return SyncFilter2.Replace("t.", "[base].").Replace("T.", "[base].");
            }
        }

        public TabularSection(Entity parent) :
            base(parent)
        {
        }

    }

    public class Field
    {
        public Entity Entity { get; set; }

        private String name;

        public String Name
        {
            get { return name; }
            set { name = value; }
        }
        private String type;

        public String Type
        {
            set { type = value; }
        }

        private bool keyField;

        public bool KeyField
        {
            get { return keyField; }
            set { keyField = value; }
        }

        private bool allowNull = true;

        public bool AllowNull
        {
            get { return allowNull; }
            set { allowNull = value; }
        }

        private bool unique;

        public bool Unique
        {
            get { return unique; }
            set { unique = value; }
        }

        private int length;

        public int Length
        {
            get { return length; }
            set { length = value; }
        }

        private int precision;

        public int Precision
        {
            get { return precision; }
            set { precision = value; }
        }

        private int scale;

        public int Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public String DataType
        {
            get
            {
                if (!SimpleType)
                    return "Guid";

                String t = model2code[type];
                if(t==null)
                    throw new Exception(String.Format("Unknown type {0}", type));

                if (!allowNull || keyField || t.Equals("String"))
                    return t;
                else
                    return String.Format("System.Nullable<{0}>", t);
            }
        }

        public String SqlType
        {
            get
            {
                if (type == null)
                    throw new Exception("Type unknown for entity " + this.Name);

                if (!SimpleType)
                {
                    return String.Format("UNIQUEIDENTIFIER {0}", allowNull && !keyField ? "NULL" : "NOT NULL");
                }
                else
                {
                    switch (type.ToUpper())
                    {
                        case "GUID":
                            return String.Format("UNIQUEIDENTIFIER {0}", allowNull && !keyField ? "NULL" : "NOT NULL");
                        case "BOOLEAN":
                            return String.Format("BIT {0}", allowNull && !keyField ? "NULL" : "NOT NULL");
                        case "INTEGER":
                            return String.Format("INT {0}", allowNull && !keyField ? "NULL" : "NOT NULL");
                        case "STRING":
                            return String.Format("VARCHAR({0}) {1}", length.ToString(), allowNull && !keyField ? "NULL" : "NOT NULL");
                        case "BLOB":
                            return String.Format("NTEXT {0}", allowNull && !keyField ? "NULL" : "NOT NULL");
                        case "DATETIME":
                            return String.Format("DATETIME {0}", allowNull && !keyField ? "NULL" : "NOT NULL");
                        case "DATETIME2":
                            return String.Format("DATETIME2 {0}", allowNull && !keyField ? "NULL" : "NOT NULL");
                        case "DECIMAL":
                            return String.Format("DECIMAL({0},{1}) {2}", precision.ToString(), scale.ToString(), allowNull && !keyField ? "NULL" : "NOT NULL");
                        default:
                            throw new Exception(String.Format("unknown data type {0}", type));
                    }
                }
            }
        }

        public String SqliteType
        {
            get
            {
                if (type == null)
                    throw new Exception("Type unknown for entity " + this.Name);

                if (!SimpleType)
                {
                    return String.Format("TEXT {0}", allowNull && !keyField ? "NULL" : "NOT NULL");
                }
                else
                {
                    switch (type.ToUpper())
                    {
                        case "GUID":
                            return String.Format("TEXT {0}", allowNull && !keyField ? "NULL" : "NOT NULL");
                        case "BOOLEAN":
                            return String.Format("INTEGER {0}", allowNull && !keyField ? "NULL" : "NOT NULL");
                        case "INTEGER":
                            return String.Format("INTEGER {0}", allowNull && !keyField ? "NULL" : "NOT NULL");
                        case "STRING":
                            return String.Format("TEXT {0}", allowNull && !keyField ? "NULL" : "NOT NULL");
                        case "BLOB":
                            return String.Format("TEXT {0}", allowNull && !keyField ? "NULL" : "NOT NULL");
                        case "DATETIME":
                            return String.Format("TEXT {0}", allowNull && !keyField ? "NULL" : "NOT NULL");
                        case "DATETIME2":
                            return String.Format("TEXT {0}", allowNull && !keyField ? "NULL" : "NOT NULL");
                        case "DECIMAL":
                            return String.Format("DECIMAL {0}", allowNull && !keyField ? "NULL" : "NOT NULL");
                        default:
                            throw new Exception(String.Format("unknown data type {0}", type));
                    }
                }
            }
        }

        public String SqlTypeShort
        {
            get
            {
                return SqlType.Split(' ')[0];
            }
        }

        public bool IsBlobField
        {
            get
            {
                return type == "Blob";
            }
        }

        public bool RefField
        {
            get
            {
                return name.ToLower().Equals("ref");
            }
        }

        public String SqlLinkedSchema
        {
            get
            {
                if (SimpleType && !keyField)
                    throw new Exception("SqlLinkedType is undefined for " + name);
                if (keyField)
                    return Entity.Schema;
                return type.Split('.')[0];
            }
        }

        public String SqlLinkedTable
        {
            get
            {
                if (SimpleType && !keyField)
                    throw new Exception("SqlLinkedType is undefined for " + name);
                if (keyField)
                    return Entity.Name;
                return type.Split('.')[1];
            }
        }

        public bool SimpleType
        {
            get
            {
                return !type.Contains('.');
            }
        }

        public Field()
        {
        }


        private static Dictionary<String, String> model2code = new Dictionary<string, string>();

        static Field()
        {
            model2code.Add("Guid", "Guid");
            model2code.Add("Boolean", "bool");
            model2code.Add("Integer", "int");
            model2code.Add("String", "String");
            model2code.Add("Blob", "String");
            model2code.Add("DateTime", "DateTime");
            model2code.Add("DateTime2", "DateTime");
            model2code.Add("Decimal", "decimal");
        }
    }

    public class Parameter : Field
    {
    }

    public class Constant
    {
        public Guid Id { get; set; }
        public String Name { get; set; }
    }

    public static class EntityHelper
    {
        public static bool ContainField(this Entity entity, String fieldName)
        {
            foreach (Field f in entity.Fields)
            {
                if (f.Name.ToLower().Equals(fieldName.ToLower()))
                    return true;
            }
            return false;
        }

        public static void Validate(this Entity entity, bool checkTabularSectionKey)
        {
            if (!entity.ContainField("Id"))
                throw new Exception(String.Format("Entity '{0}' does not contain mandatory field 'Id'", entity.Name));

            foreach (TabularSection ts in entity.TabularSections)
            {
                if(checkTabularSectionKey && String.IsNullOrEmpty(ts.Key))
                    throw new Exception(String.Format("Tabular section '{0}.{1}' does not have 'Key' attribute", entity.Name, ts.Name));
                if (!ts.ContainField("LineNumber"))
                    throw new Exception(String.Format("Tabular section '{0}.{1}' does not contain mandatory field 'LineNumber'", entity.Name, ts.Name));
                if (!ts.ContainField("Ref"))
                    throw new Exception(String.Format("Tabular section '{0}.{1}' does not contain mandatory field 'Ref'", entity.Name, ts.Name));
            }
        }

    }

    public class FieldList : List<Field>
    {
        private Entity entity;

        public FieldList(Entity entity)
        {
            this.entity = entity;
        }

        public new void Add(Field f)
        {
            base.Add(f);
            f.Entity = entity;
        }
    }
}
