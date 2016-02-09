package com.firstbit.bitmobile.server.admin.core;

import java.util.ArrayList;
import java.util.List;

import com.firstbit.bitmobile.utils.StringUtils;

public class TabularSection
{
    private String name;
    private String key;
    private List<Entity> entities;

    public TabularSection(String name, String key)
    {
        this.name = name;
        this.key = key;
        entities = new ArrayList<Entity>();
    }

    public String getName()
    {
        return name;
    }

    public String getKey()
    {
        return key;
    }

    public List<Entity> getEntities()
    {
        return entities;
    }

    public void AddEntity(Entity entity)
    {
        entities.add(entity);
    }

    public String[] getKeyFields()
    {
        if (StringUtils.isNullOrEmpty(key))
            return null;
        else
            return key.split("\\,");
    }
}
