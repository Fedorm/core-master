package com.firstbit.bitmobile.server.codeFactory;

import java.lang.reflect.Method;

import org.w3c.dom.*;

public class XmlHelper 
{
	public static NodeList select(Element root, String path)
	{
		NodeList result = null;
		String[] arr = path.split("/");
		for(String s:arr)
		{
			if(s.length()>0)
			{
				result = root.getElementsByTagName(s);
				if(result.getLength()==0)
					return result;
				root = (Element) result.item(0);
			}
		}
		return result;
	}
	
	public static void setAttributes(Node node, Object entity) throws Exception
	{
		Class<?>[] arr = new Class[] { String.class, Integer.class, Boolean.class };
		
		Class<?> type = entity.getClass();
        NamedNodeMap attributes = node.getAttributes();
        for(int i=0;i<attributes.getLength();i++)
        {
        	Node attr = attributes.item(i);

        	String methodName = "set" + attr.getNodeName();
        	Method method = null;
        	for(Class<?> c:arr)
        	{
        		try
        		{
                	method = type.getMethod(methodName, new Class[] { c });
        			if(method!=null)
        			{
        				method.invoke(entity, new Object[]{cast(attr.getNodeValue(), c)});   
        				break;
        			}
        		}
        		catch(NoSuchMethodException e)
        		{        			
        		}
        	}
        	
        	if(method == null)
        		throw new Exception(String.format("no such method %s", methodName));        	
        }  		
	}
	
	
	private static Object cast(String value, Class<?> type) throws Exception
	{
		if(type.equals(String.class))
			return value;
		if(type.equals(Integer.class))
			return Integer.parseInt(value);
		if(type.equals(Boolean.class))
			return Boolean.parseBoolean(value);
		
		throw new Exception(String.format("Unable to cast %s to %s", value, type.getCanonicalName()));
	}	
}
