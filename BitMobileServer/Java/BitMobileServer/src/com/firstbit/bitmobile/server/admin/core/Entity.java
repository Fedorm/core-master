package com.firstbit.bitmobile.server.admin.core;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map.Entry;

import com.firstbit.bitmobile.server.dao.IEntity;
import com.firstbit.bitmobile.utils.StringUtils;

public class Entity implements IEntity
{
    private HashMap<String, String> attributes;
    private HashMap<String, TabularSection> tabularSections;
    private TabularSection currentTabularSection;
    private int depth;

    public Entity(int depth)
    {
        this.depth = depth;
        this.attributes = new HashMap<String, String>();
    }

    public void AddTabularSection(String name)
    {
    	AddTabularSection(name, null);
    }
    
    public void AddTabularSection(String name, String key)
    {
        if (!StringUtils.isNullOrEmpty(name))
        {
            if (tabularSections == null)
                tabularSections = new HashMap<String, TabularSection>();
            currentTabularSection = new TabularSection(name, key);
            tabularSections.put(name, currentTabularSection);
        }
    }

    public List<Entry<String, TabularSection>> GetTabularSections()
    {
    	ArrayList<Entry<String, TabularSection>> result = new ArrayList<Entry<String, TabularSection>>();
        if (tabularSections != null)
        {
            for(Entry<String, TabularSection> item:tabularSections.entrySet())
                result.add(item);
        }
        return result;
    }

    public void Clear()
    {
        attributes.clear();
        tabularSections.clear();
    }
    
    public HashMap<String, String> getAttributes()
    {
    	return this.attributes;
    }

    public Boolean hasTabularSections()
    {
    	return this.tabularSections!=null;
    }
    
    public HashMap<String, TabularSection> getTabularSections()
    {
    	return this.tabularSections;
    }
    
    public TabularSection getCurrentTabularSection()
    {
    	return this.currentTabularSection;
    }
    
    public int getDepth()
    {
    	return this.depth;
    }

	@Override
	public String getAttribute(String name) {
		return attributes.get(name);
	}
}
