package com.firstbit.bitmobile.server.codeFactory;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import com.firstbit.bitmobile.utils.StringUtils;

public class TabularSection extends Entity
{
	private String key;

	public String getKey() {
		return key;
	}

	public void setKey(String key) {
		this.key = key;
	}
	
    public TabularSection(Entity parent)
    {
    	super(parent);
    }	
    
    public Boolean hasTabularSectionKey()
    {
        return !StringUtils.isNullOrEmpty(key);
    }    

    public Boolean getHasTabularSectionKey()
    {
        return !StringUtils.isNullOrEmpty(key);
    }    
    
    public List<Field> getTabularSectionKeys()
    {
    	List<Field> result = new ArrayList<Field>();
        if (!StringUtils.isNullOrEmpty(key))
        {
            for(String s:key.split(","))
            {
                String keyName = s.trim();
                for(Field f:getFields())
                {
                    if (f.getName().equalsIgnoreCase(keyName))
                        result.add(f);
                }
            }
        }
        return result;
    }
    
    public List<Field> getTabularFieldsExceptKeys()
    {
    	HashMap<String, Field> keys = new HashMap<String, Field>();
        for (Field f:getTabularSectionKeys())
        {
            keys.put(f.getName().toLowerCase(), f);
        }

    	List<Field> result = new ArrayList<Field>();
        for(Field f:getFields())
        {
            if (!f.getKeyField() && !f.getName().equalsIgnoreCase("ref") && !keys.containsKey(f.getName().toLowerCase()))
                result.add(f);
        }
        return result;
    }    
    
}
