using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminService
{
    class FastXmlReader
    {
        public static Dictionary<String, String> ReadRegionalSettings(System.Xml.XmlTextReader r)
        {
            while (r.Read())
            {
                if (r.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (r.Name.ToLower().Equals("regionalsettings"))
                    {
                        Entity rs = ReadRow(null, r);
                        return rs.Attributes;
                    }

                    if (r.Name.ToLower().Equals("rows"))
                        return null;
                }
            }
            return null;
        }

        public static IEnumerable<Entity> ReadRow(System.Xml.XmlTextReader r)
        {
            Entity lastEntity = null;
            while (r.Read())
            {
                if (r.NodeType == System.Xml.XmlNodeType.Element)
                {
                    if (r.Name.ToLower().Equals("row"))
                    {
                        if (lastEntity != null)
                        {
                            if (r.Depth == lastEntity.Depth)
                            {
                                yield return lastEntity;
                                lastEntity = ReadRow(null, r);
                            }
                            else
                            {
                                ReadRow(lastEntity, r);
                            }
                        }
                        else
                            lastEntity = ReadRow(null, r);
                    }
                    else
                    {
                        if (lastEntity != null && (r.Depth == lastEntity.Depth + 1))
                            ReadRow(lastEntity, r);
                    }
                }
            }
            if (lastEntity != null)
                yield return lastEntity;
        }

        private static Entity ReadRow(Entity parent, System.Xml.XmlTextReader r)
        {
            if (parent != null && r.AttributeCount <= 1) //tabular section, no attributes or "key"
            {
                String tsName = r.Name;

                if (r.MoveToFirstAttribute())
                {
                    if (r.Name.ToLower().Equals("key")) //only key attrbute is allowed
                        parent.AddTabularSection(tsName, r.Value);
                    else
                        throw new Exception(String.Format("Invalid tabular section attribute '{0}'", r.Name));
                }
                else
                    parent.AddTabularSection(tsName);

                return parent;
            }
            else
            {
                Entity entity = new Entity(r.Depth);
                if (r.MoveToFirstAttribute())
                {
                    do
                    {
                        String name = r.Name;
                        r.ReadAttributeValue();
                        entity.Attributes.Add(name, r.ReadContentAsString());
                    }
                    while (r.MoveToNextAttribute());
                }

                if (parent != null)
                {
                    parent.CurrentTabularSection.AddEntity(entity);
                    return parent;
                }
                else
                    return entity;
            }
        }
    }

    class Entity
    {
        public Dictionary<String, String> Attributes { get; private set; }
        private Dictionary<String, TabularSection> TabularSections { get; set; }
        public TabularSection CurrentTabularSection { get; private set; }
        public int Depth { get; private set; }

        public Entity(int depth)
        {
            Depth = depth;
            Attributes = new Dictionary<string, string>();
        }

        public void AddTabularSection(String name, String key = null)
        {
            if (!String.IsNullOrEmpty(name))
            {
                if (TabularSections == null)
                    TabularSections = new Dictionary<string, TabularSection>();
                CurrentTabularSection = new TabularSection(name, key);
                TabularSections.Add(name, CurrentTabularSection);
            }
        }

        public IEnumerable<KeyValuePair<String, TabularSection>> GetTabularSections()
        {
            if (TabularSections != null)
            {
                foreach (var item in TabularSections)
                    yield return item;
            }
        }

        public void Clear()
        {
            Attributes.Clear();
            TabularSections.Clear();
        }
    }

    class TabularSection
    {
        private String name;
        private String key;
        private List<Entity> entities;

        public TabularSection(String name, String key)
        {
            this.name = name;
            this.key = key;
            entities = new List<Entity>();
        }

        public String Name
        {
            get
            {
                return name;
            }
        }

        public String Key
        {
            get
            {
                return key;
            }
        }

        public List<Entity> Entities
        {
            get
            {
                return entities;
            }
        }

        public void AddEntity(Entity entity)
        {
            entities.Add(entity);
        }

        public String[] KeyFields
        {
            get
            {
                if (String.IsNullOrEmpty(key))
                    return null;
                else
                    return key.Split(',');
            }
        }
    }

}
