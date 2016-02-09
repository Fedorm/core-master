using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace CodeFactory
{
    public class Factory
    {
        public ConfigVersion GetVersion(String fileName)
        {
            ConfigVersion result = new ConfigVersion();
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            XmlAttribute aName = doc.DocumentElement.Attributes["Name"];
            XmlAttribute aVersion = doc.DocumentElement.Attributes["Version"];
            if (aName == null)
                throw new Exception("Configuration node must contain 'Name' attribute.");
            if (aVersion == null)
                throw new Exception("Configuration node must contain 'Version' attribute.");

            result.Name =aName.Value;
            result.Version = aVersion.Value;
            return result;
        }

        public List<Entity> GetEntities(String fileName, bool checkTabularSectionKey = false)
        {
            List<Entity> result = new List<Entity>();
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            XmlNodeList nodes = doc.DocumentElement.SelectNodes("//Entities/Entity");
            foreach (XmlNode entityNode in nodes)
            {
                Entity e = CreateEntity(entityNode, true, checkTabularSectionKey);
                result.Add(e);
            }
            return result;
        }

        public Dictionary<String, List<Constant>> GetConstants(String fileName)
        {
            Dictionary<String, List<Constant>> result = new Dictionary<string, List<Constant>>();
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            XmlNodeList entityNodes = doc.DocumentElement.SelectNodes("//Configuration/Constants/Entity");
            foreach (XmlNode entityNode in entityNodes)
            {
                String entityName = entityNode.Attributes["Name"].Value;
                entityName = entityName.Split('.')[1];
                List<Constant> list = new List<Constant>();
                result.Add(entityName, list);

                System.Xml.XmlNodeList rows = entityNode.SelectNodes("Row");
                foreach (System.Xml.XmlNode row in rows)
                {
                    Constant c = new Constant();
                    c.Id = new Guid(row.Attributes["Id"].Value);
                    c.Name = row.Attributes["Name"].Value.Replace(' ', '_');
                    list.Add(c);
                }
            }
            return result;
        }

        public List<Entity> GetResources(String directory)
        {
            List<String> entities = new List<string>();
            entities.Add("Configuration");
            entities.Add("BusinessProcess");
            entities.Add("Image");
            entities.Add("Screen");
            entities.Add("Script");
            entities.Add("Style");
            entities.Add("Translation");

            String schema = "resource";// new System.IO.DirectoryInfo(directory).Name;
            List<Entity> result = new List<Entity>();
            foreach (String entity in entities)
            {
                Entity e = new Entity(null);
                e.Schema = schema;
                e.Name = entity;

                Field f;

                f = new Field();
                f.Name = "Id";
                f.Type = "Guid";
                f.KeyField = true;
                e.Fields.Add(f);

                f = new Field();
                f.Name = "Name";
                f.Type = "String";
                f.AllowNull = false;                
                f.Length = 250;
                e.Fields.Add(f);

                f = new Field();
                f.Name = "Data";
                f.Type = "Blob";
                f.AllowNull = false;
                e.Fields.Add(f);

                f = new Field();
                f.Name = "Parent";
                f.Type = "String";
                f.AllowNull = false;
                f.Length = 250;
                e.Fields.Add(f);

                result.Add(e);
            }

            return result;
        }

        public List<Entity> GetAdmin()
        {
            List<Entity> result = new List<Entity>();
            Entity e = new Entity(null);
            e.Schema = "admin";
            e.Name = "Entity";

            Field f;

            f = new Field();
            f.Name = "Id";
            f.Type = "Guid";
            f.KeyField = true;
            e.Fields.Add(f);

            f = new Field();
            f.Name = "Name";
            f.Type = "String";
            f.AllowNull = false;
            f.Unique = true;
            f.Length = 250;
            e.Fields.Add(f);

            f = new Field();
            f.Name = "Schema";
            f.Type = "String";
            f.AllowNull = false;
            f.Unique = false;
            f.Length = 50;
            e.Fields.Add(f);

            f = new Field();
            f.Name = "ShortName";
            f.Type = "String";
            f.AllowNull = false;
            f.Unique = false;
            f.Length = 50;
            e.Fields.Add(f);

            result.Add(e);

            return result;
        }

        public String GetScope(String fileName)
        {
            Dictionary<String, Parameter> result = new Dictionary<String, Parameter>();
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            XmlNode node = doc.DocumentElement;
            return node.Attributes["Name"].Value;
        }

        public Dictionary<String, Parameter> GetGlobalParameters(String fileName)
        {
            Dictionary<String, Parameter> result = new  Dictionary<String, Parameter>();

            //userId parameter
            Parameter userId = new Parameter();
            userId.Name = "UserId";
            userId.Type = "Guid";
            userId.AllowNull = false;
            result.Add(userId.Name, userId);
            
            //from config..
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            XmlNodeList nodes = doc.DocumentElement.SelectNodes("//GlobalParameters/Parameter");
            foreach (XmlNode parameterNode in nodes)
            {
                Parameter e = CreateParameter(parameterNode);
                result.Add(e.Name, e);
            }
            return result;
        }

        private Entity CreateEntity(XmlNode node, bool hasDeletedField = false, bool checkTabularSectionKey = false)
        {
            Entity entity = new Entity(null, hasDeletedField);
            foreach (System.Xml.XmlAttribute a in node.Attributes)
            {
                System.Reflection.PropertyInfo pi = entity.GetType().GetProperty(a.Name);
                pi.SetValue(entity, System.Convert.ChangeType(a.Value, pi.PropertyType), null);
            }

            XmlNodeList fieldNodes = node.SelectNodes("Fields/Field");
            foreach (XmlNode fieldNode in fieldNodes)
            {
                Field field = new Field();
                foreach (System.Xml.XmlAttribute a in fieldNode.Attributes)
                {
                    System.Reflection.PropertyInfo pi = field.GetType().GetProperty(a.Name);
                    pi.SetValue(field, System.Convert.ChangeType(a.Value, pi.PropertyType), null);
                }
                entity.Fields.Add(field);
            }
            entity.SortFields();

            XmlNodeList tabularSectionNodes = node.SelectNodes("TabularSections/TabularSection");
            foreach (XmlNode tabularSectionNode in tabularSectionNodes)
            {
                TabularSection tabularSection = new TabularSection(entity);
                foreach (System.Xml.XmlAttribute a in tabularSectionNode.Attributes)
                {
                    System.Reflection.PropertyInfo pi = tabularSection.GetType().GetProperty(a.Name);
                    pi.SetValue(tabularSection, System.Convert.ChangeType(a.Value, pi.PropertyType), null);
                }
         
                XmlNodeList tabularSectionFieldNodes = tabularSectionNode.SelectNodes("Field");
                foreach (XmlNode tabularSectionFieldNode in tabularSectionFieldNodes)
                {
                    Field field = new Field();
                    foreach (System.Xml.XmlAttribute a in tabularSectionFieldNode.Attributes)
                    {
                        System.Reflection.PropertyInfo pi = field.GetType().GetProperty(a.Name);
                        pi.SetValue(field, System.Convert.ChangeType(a.Value, pi.PropertyType), null);
                    }
                    tabularSection.Fields.Add(field);
                }

                tabularSection.SortFields();
                entity.TabularSections.Add(tabularSection);
            }

            entity.Validate(checkTabularSectionKey);
            return entity;
        }

        private Parameter CreateParameter(XmlNode node)
        {
            Parameter parameter = new Parameter();
            foreach (System.Xml.XmlAttribute a in node.Attributes)
            {
                System.Reflection.PropertyInfo pi = parameter.GetType().GetProperty(a.Name);
                pi.SetValue(parameter, System.Convert.ChangeType(a.Value, pi.PropertyType), null);
            }
            return parameter;
        }
    }

}
