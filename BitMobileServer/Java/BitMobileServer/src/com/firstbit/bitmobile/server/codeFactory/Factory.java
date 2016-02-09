package com.firstbit.bitmobile.server.codeFactory;

import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.UUID;

import org.w3c.dom.*;

public class Factory 
{
    public ConfigVersion GetVersion(Document doc) throws Exception
    {
        ConfigVersion result = new ConfigVersion();

        Element root = doc.getDocumentElement();
        
        String aName = root.getAttribute("Name");
        String aVersion = root.getAttribute("Version");
        if (aName == null)
            throw new Exception("Configuration node must contain 'Name' attribute.");
        if (aVersion == null)
            throw new Exception("Configuration node must contain 'Version' attribute.");

        result.setName(aName);
        result.setVersion(aVersion);
        
        return result;
    }	
    
    public HashMap<String, Parameter> getGlobalParameters(Document doc) throws Exception
    {
    	HashMap<String, Parameter> result = new HashMap<String, Parameter>();

        //userId parameter
        Parameter userId = new Parameter();
        userId.setName("UserId");
        userId.setType("Guid");
        userId.setAllowNull(false);
        result.put(userId.getName(), userId);

        //from config..
        NodeList nodes = XmlHelper.select(doc.getDocumentElement(), "GlobalParameters/Parameter");
        Node node = null;
        for(int i=0;i<nodes.getLength();i++)
        {
        	node = nodes.item(i);
			if(node.getNodeType()==Node.ELEMENT_NODE)
			{
				Element pNode = (Element)node;
				Parameter e = CreateParameter(pNode);
				result.put(e.getName(), e);
			}
        }
        return result;
    }
 
    private Parameter CreateParameter(Element node) throws Exception
    {
        Parameter parameter = new Parameter();
        Class<?> type = Parameter.class;
        
        NamedNodeMap attributes = node.getAttributes();
        for(int i=0;i<attributes.getLength();i++)
        {
        	Node attr = attributes.item(i);

        	Method method = type.getMethod("set" + attr.getLocalName());
			if(method!=null)
				method.invoke(parameter, new Object[]{attr.getNodeValue()});
        }
        return parameter;
    }
    
    public List<Entity> GetEntities(Document doc, Boolean checkTabularSectionKey) throws Exception
    {
        List<Entity> result = new ArrayList<Entity>();

        NodeList nodes = XmlHelper.select(doc.getDocumentElement(), "Entities/Entity");
        Node node = null;
        for(int i=0;i<nodes.getLength();i++)
        {
        	node = nodes.item(i);
			if(node.getNodeType()==Node.ELEMENT_NODE)
			{
				Element eNode = (Element)node;
				Entity e = CreateEntity(eNode, checkTabularSectionKey);
				result.add(e);
			}
        }
        return result;
    }
    
    private Entity CreateEntity(Node node, Boolean checkTabularSectionKey) throws Exception
    {
        Entity entity = new Entity(null);
        XmlHelper.setAttributes(node, entity);
        
        NodeList fieldNodes = XmlHelper.select((Element) node, "Fields/Field");
        Node fNode = null;
        for(int i=0;i<fieldNodes.getLength();i++)
        {
        	fNode = fieldNodes.item(i);

			Field field = new Field();
			if(fNode.getNodeType() == Node.ELEMENT_NODE)
        	{
				Element fieldNode = (Element)fNode;        	
				XmlHelper.setAttributes(fieldNode, field);
        	}
        	entity.getFields().add(field);      	
        }
        entity.SortFields();

        NodeList tabularSectionNodes = XmlHelper.select((Element) node, "TabularSections/TabularSection");
        Node tsNode = null;
        for(int i=0;i<tabularSectionNodes.getLength();i++)
        {
        	tsNode = tabularSectionNodes.item(i);

			TabularSection tSection = new TabularSection(entity);
			if(tsNode.getNodeType() == Node.ELEMENT_NODE)
        	{
				Element tabularSectionNode = (Element)tsNode;        	
				XmlHelper.setAttributes(tsNode, tSection);
		        
		        NodeList tabularSectionFieldNodes = XmlHelper.select(tabularSectionNode, "Field");
		        Node tsfNode = null;
		        for(int j=0;j<tabularSectionFieldNodes.getLength();j++)
		        {
		        	Field field = new Field();

		        	tsfNode = tabularSectionFieldNodes.item(j);		        	
					XmlHelper.setAttributes(tsfNode, field);		        	
			        tSection.getFields().add(field);
		        }
		        
		        tSection.SortFields();
		        entity.getTabularSections().add(tSection);
		        
        	}			
        }
        
    	entity.Validate(checkTabularSectionKey);
    	return entity;           
    }        
    
    public List<Entity> GetResources() throws Exception
    {
        List<String> entities = new ArrayList<String>();
        entities.add("Configuration");
        entities.add("BusinessProcess");
        entities.add("Image");
        entities.add("Screen");
        entities.add("Script");
        entities.add("Style");
        entities.add("Translation");

        String schema = "resource";// new System.IO.DirectoryInfo(directory).Name;
        List<Entity> result = new ArrayList<Entity>();
        for(String entity:entities)
        {
            Entity e = new Entity(null);
            e.setSchema(schema);
            e.setName(entity);

            Field f;

            f = new Field();
            f.setName("Id");
            f.setType("Guid");
            f.setKeyField(true);
            e.getFields().add(f);

            f = new Field();
            f.setName("Name");
            f.setType("String");
            f.setAllowNull(false);                
            f.setLength(250);
            e.getFields().add(f);

            f = new Field();
            f.setName("Data");
            f.setType("Blob");
            f.setAllowNull(false);
            e.getFields().add(f);

            f = new Field();
            f.setName("Parent");
            f.setType("String");
            f.setAllowNull(false);
            f.setLength(250);
            e.getFields().add(f);

            if (e.getName().equalsIgnoreCase("image") || e.getName().equalsIgnoreCase("style"))
            {
                e.setSyncFilter("t.Parent LIKE '%' + @Resolution + '%'");
            }

            result.add(e);
        }

        return result;    
    }
    
    public List<Entity> GetAdmin()
    {
        List<Entity> result = new ArrayList<Entity>();
        Entity e = new Entity(null);
        e.setSchema("admin");
        e.setName("Entity");

        Field f;

        f = new Field();
        f.setName("Id");
        f.setType("Guid");
        f.setKeyField(true);
        e.getFields().add(f);

        f = new Field();
        f.setName("Name");
        f.setType("String");
        f.setAllowNull(false);
        f.setUnique(true);
        f.setLength(250);
        e.getFields().add(f);

        f = new Field();
        f.setName("Schema");
        f.setType("String");
        f.setAllowNull(false);
        f.setUnique(false);
        f.setLength(50);
        e.getFields().add(f);

        f = new Field();
        f.setName("ShortName");
        f.setType("String");
        f.setAllowNull(false);
        f.setUnique(false);
        f.setLength(50);
        e.getFields().add(f);

        result.add(e);

        return result;
    }
    
    public HashMap<String, List<Constant>> GetConstants(Document doc)
    {
    	HashMap<String, List<Constant>> result = new HashMap<String, List<Constant>>();
        NodeList nodes = XmlHelper.select(doc.getDocumentElement(), "Configuration/Constants/Entity");
        Node node = null;
        for(int i=0;i<nodes.getLength();i++)
        {
        	node = nodes.item(i);
			if(node.getNodeType()==Node.ELEMENT_NODE)
			{
				Element eNode = (Element)node;
				
	            String entityName = eNode.getAttribute("Name");
	            entityName = entityName.split("\\.")[1];
	            List<Constant> list = new ArrayList<Constant>();
	            result.put(entityName, list);

	            NodeList rows = XmlHelper.select(eNode,"Row");
	            for(int j=0;i<rows.getLength();j++)
	            {
	            	node = rows.item(j);
	    			if(node.getNodeType()==Node.ELEMENT_NODE)
	    			{
	    				Element rNode = (Element)node;

	    				Constant c = new Constant();
	                    c.setId(UUID.fromString(rNode.getAttribute("Id")));
	                    c.setName(rNode.getAttribute("Name").replace(' ', '_'));
	                    list.add(c);
	    			}
	            }
			}
        }
        return result;        
    }   
    
}
