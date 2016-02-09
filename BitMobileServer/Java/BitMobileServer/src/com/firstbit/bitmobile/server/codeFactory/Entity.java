package com.firstbit.bitmobile.server.codeFactory;

import java.util.ArrayList;
import java.util.List;

import com.firstbit.bitmobile.utils.StringUtils;

public class Entity 
{
	private Entity parent;   
    private String schema;
    private String name;
    private Boolean syncUpload;
    private Boolean syncDownload;
    private FieldList fields;
    private List<TabularSection> tabularSections;
    private String syncFilter;
    
    public Entity(Entity parent)
    {
        this.parent = parent;
        this.fields = new FieldList(this);
        this.tabularSections = new ArrayList<TabularSection>();
    }
    
	public String getSchema() {
		return schema;
	}
	public void setSchema(String schema) {
		this.schema = schema;
	}
	public String getName() {
		return name;
	}
	public void setName(String name) {
		this.name = name;
	}
	public Boolean getSyncUpload() {
		return syncUpload;
	}
	public void setSyncUpload(Boolean syncUpload) {
		this.syncUpload = syncUpload;
	}
	public Boolean getSyncDownload() {
		return syncDownload;
	}
	public void setSyncDownload(Boolean syncDownload) {
		this.syncDownload = syncDownload;
	}  
	public FieldList getFields()
	{
		return this.fields;
	}
	public List<TabularSection> getTabularSections()
	{
		return this.tabularSections;
	}
	
    public void SortFields()
    {
        List<Field> fs = new ArrayList<Field>();
        String order = "Id,Ref,LineNumber";
        String[] arr = order.split(",");

        for(String s:arr)
        {
            for (int j = 0; j < this.getFields().size(); j++)
            {
                Field f = this.getFields().get(j);
                if (f.getName().equalsIgnoreCase(s))
                {
                    getFields().remove(f);
                    fs.add(0, f);
                    break;
                }
            }
        }

        for(Field f:fs)
        {
            getFields().add(0, f);
        }
    }	
    
    public void Validate(Boolean checkTabularSectionKey) throws Exception
    {
        if (!ContainField("Id"))
            throw new Exception(String.format("Entity '%s' does not contain mandatory field 'Id'", getName()));

        for(TabularSection ts:getTabularSections())
        {
            if(checkTabularSectionKey && StringUtils.isNullOrEmpty(ts.getKey()))
                throw new Exception(String.format("Tabular section '%s.%S' does not have 'Key' attribute", getName(), ts.getName()));
            if (!ts.ContainField("LineNumber"))
                throw new Exception(String.format("Tabular section '%s.%s' does not contain mandatory field 'LineNumber'", getName(), ts.getName()));
            if (!ts.ContainField("Ref"))
                throw new Exception(String.format("Tabular section '%S.%s' does not contain mandatory field 'Ref'", getName(), ts.getName()));
        }
    } 
    
    public Boolean ContainField(String fieldName)
    {
        for(Field f:getFields())
        {
            if (f.getName().equalsIgnoreCase(fieldName))
                return true;
        }
        return false;
    }   

	public String getSyncFilter() {
		return syncFilter;
	}

	public void setSyncFilter(String syncFilter) {
		this.syncFilter = syncFilter;
	}
	
    public Boolean isLastField(Field field)
    {
        return this.getFields().get(getFields().size() - 1).equals(field);
    }	
    
    public String getKeyField() throws Exception
    {
        for(Field item:fields)
        {
            if (item.getKeyField())
                return item.getName();
        }
        throw new Exception(String.format("Key field is not found for entity:{%s}", this.getName()));
    }    
    
    public List<Field> getFieldsExceptKey()
    {
    	List<Field> result = new ArrayList<Field>();
        for(Field f:fields)
        {
            if (!f.getKeyField())
                result.add(f);
        }
        return result;
    }    
}
