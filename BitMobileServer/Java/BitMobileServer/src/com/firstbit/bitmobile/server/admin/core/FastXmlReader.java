package com.firstbit.bitmobile.server.admin.core;

import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.sql.SQLException;
import java.text.ParseException;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.parsers.SAXParser;
import javax.xml.parsers.SAXParserFactory;

import org.xml.sax.helpers.DefaultHandler; 
import org.xml.sax.*; 

import org.apache.commons.io.input.BOMInputStream;

public class FastXmlReader extends DefaultHandler implements ErrorHandler
{
	private int level = 0;
	
	private IEntityReadCallback readCallback;
	private Object ctx;

	private Entity lastEntity = null;
	private Boolean regionalSettngsRead = false;

	public void start(InputStream is, IEntityReadCallback readCallback, Object ctx) throws SAXException
	{
		this.readCallback = readCallback;
		this.ctx = ctx;
		
		try
		{
			SAXParserFactory factory = SAXParserFactory.newInstance(); 
			SAXParser parser = factory.newSAXParser(); 
			
			InputSource s = new InputSource(new InputStreamReader(new BOMInputStream(is),"UTF-8"));
			s.setEncoding("UTF-8");
			
			parser.parse(s, this);
		}
		catch(ParserConfigurationException | SAXException | IOException e)
		{
			throw new SAXException(e);
		}
	}
	
	@Override 
	public void startDocument() throws SAXException 
	{
	} 	

	@Override 
	public void startElement(String namespaceURI, String localName, String qName, Attributes atts) throws SAXException 
	{ 
		level++;
		
		if(!regionalSettngsRead)
		{
			if(level == 2 && qName.equalsIgnoreCase("RegionalSettings"))
			{
				readCallback.regionalSettingsRead(readRow(null, qName, atts).getAttributes(), ctx);
				regionalSettngsRead = true;
				return;
			}
		}
		
		if(qName.equalsIgnoreCase("row"))
		{
			if(lastEntity!=null)
				readRow(lastEntity, qName, atts);
			else
				lastEntity = readRow(null, qName, atts);
		}
		else
		{
            if (lastEntity != null && (level == lastEntity.getDepth() + 1))
                readRow(lastEntity, qName, atts);			
		}
	} 	

	@Override 
	public void endElement(String namespaceURI, String localName, String qName) throws SAXException 
	{ 
		level--;
		
		if(level == 2 && qName.equalsIgnoreCase("row") && lastEntity!=null)
		{
			try 
			{
				readCallback.entityRead(lastEntity, ctx);
				lastEntity = null;
			} 
			catch (SQLException | ParseException e) 
			{
				throw new SAXException(e);
			}
		}
	}	
	
	private Entity readRow(Entity parent, String name, Attributes atts) throws SAXException
	{
        if (parent != null && atts.getLength() <= 1) //tabular section, no attributes or "key"
        {
            String tsName = name;

            if(atts.getLength() == 1)
            {
            	String aName = atts.getQName(0);
            	if(aName.equalsIgnoreCase("key"))
                    parent.AddTabularSection(tsName, atts.getValue(0));
                else
                    throw new SAXException(String.format("Invalid tabular section attribute '%s'", aName));            		
            }
            else
                parent.AddTabularSection(tsName);

            return parent;
        }
        else
        {
            Entity entity = new Entity(level);
			for(int i=0;i<atts.getLength();i++)
			{
				String aName = atts.getQName(i);
				String aValue = atts.getValue(i);
				entity.getAttributes().put(aName, aValue);
			}
            
            if (parent != null)
            {
                parent.getCurrentTabularSection().AddEntity(entity);
                return parent;
            }
            else
                return entity;
        }
	}
	
}
