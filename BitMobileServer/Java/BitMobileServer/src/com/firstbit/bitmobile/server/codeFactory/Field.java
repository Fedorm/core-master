package com.firstbit.bitmobile.server.codeFactory;

import java.util.HashMap;
import java.util.UUID;

public class Field 
{
	private Entity entity;

	public Entity getEntity() {
		return entity;
	}

	public void setEntity(Entity entity) {
		this.entity = entity;
	}
	
	private String name;	
	
    public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	private String type;
	
	public String getType()
	{
		return type;
	}
	
	public void setType(String value)
	{
		this.type = value;
	}
	
	private Boolean keyField;	
	
    public Boolean getKeyField() {
		return keyField == null ? false : keyField;
	}

	public void setKeyField(Boolean keyField) {
		this.keyField = keyField;
	}
	
	private Boolean allowNull = true;	
	
	public Boolean getAllowNull() {
		return allowNull == null ? false : allowNull;
	}

	public void setAllowNull(Boolean allowNull) {
		this.allowNull = allowNull;
	}

	private Boolean unique;
	
	public Boolean getUnique() {
		return unique == null ? false : unique;
	}

	public void setUnique(Boolean unique) {
		this.unique = unique;
	}

	private Integer length;
	
	public Integer getLength() {
		return length == null ? 0 : length;
	}

	public void setLength(Integer length) {
		this.length = length;
	}
	
	private Integer precision;
	
	public Integer getPrecision() {
		return precision == null ? 0 : precision;
	}

	public void setPrecision(Integer precision) {
		this.precision = precision;
	}
	
	private Integer scale;
	
	public Integer getScale() {
		return scale == null ? 0 : scale;
	}

	public void setScale(Integer scale) {
		this.scale = scale;
	}

	public String getDataType() throws Exception
	{
        if (!getSimpleType())
            return "Guid";

        String t = getModel2Code().get(getType());
        if(t==null)
            throw new Exception(String.format("Unknown type %s", getType()));

        if (!getAllowNull() || getKeyField() || t.equalsIgnoreCase("String"))
            return t;
        else
            return String.format("System.Nullable<%s>", t);	
    }

	public String getSqlType() throws Exception
	{
        if (type == null)
            throw new Exception("Type unknown for entity " + this.getName());

        if (!getSimpleType())
        {
            return String.format("VARCHAR(36) %s", getAllowNull() && !getKeyField() ? "NULL" : "NOT NULL");
        }
        else
        {
            switch (getType().toUpperCase())
            {
                case "GUID":
                    return String.format("VARCHAR(36) %s", getAllowNull() && !getKeyField() ? "NULL" : "NOT NULL");
                case "BOOLEAN":
                    return String.format("BOOL %s", getAllowNull() && !getKeyField() ? "NULL" : "NOT NULL");
                case "INTEGER":
                    return String.format("INT %s", getAllowNull() && !getKeyField() ? "NULL" : "NOT NULL");
                case "STRING":
                    return String.format("VARCHAR(%d) %s", getLength(), getAllowNull() && !getKeyField() ? "NULL" : "NOT NULL");
                case "BLOB":
                    return String.format("LONGTEXT %s", getAllowNull() && !getKeyField() ? "NULL" : "NOT NULL");
                case "DATETIME":
                    return String.format("DATETIME %s", getAllowNull() && !getKeyField() ? "NULL" : "NOT NULL");
                case "DATETIME2":
                    return String.format("DATETIME %s", getAllowNull() && !getKeyField() ? "NULL" : "NOT NULL");
                case "DECIMAL":
                    return String.format("DECIMAL(%d,%d) %s", getPrecision(), getScale(), getAllowNull() && !getKeyField() ? "NULL" : "NOT NULL");
                default:
                    throw new Exception(String.format("unknown data type %s", getType()));
            }
        }		
	}
	
    public String getSqlTypeShort() throws Exception
    {
    	return getSqlType().split(" ")[0];
    }	
	
    public Boolean getIsBlobField()
    {
    	return getType().equalsIgnoreCase("Blob");
    }    
    
    public Boolean getRefField()
    {
    	return getName().equalsIgnoreCase("ref");
    }    
    
    public String getSqlLinkedSchema() throws Exception
    {
        if (getSimpleType() && !getKeyField())
            throw new Exception("SqlLinkedType is undefined for " + name);
        if (getKeyField())
            return getEntity().getSchema();
        return getType().split("\\.")[0];
    }    
    
    public String getSqlLinkedTable() throws Exception
    {
        if (getSimpleType() && !getKeyField())
            throw new Exception("SqlLinkedType is undefined for " + name);
        if (getKeyField())
            return getEntity().getName();
        return getType().split("\\.")[1];
    }   
    
    public Boolean getSimpleType()
    {
        return !getType().contains(".");
    }
   
    private static HashMap<String, String> model2code = null;
    
    public static HashMap<String, String> getModel2Code()
    {
    	if(model2code == null)
    	{
    		model2code = new HashMap<String, String>();
            model2code.put("Guid", "Guid");
            model2code.put("Boolean", "bool");
            model2code.put("Integer", "int");
            model2code.put("String", "String");
            model2code.put("Blob", "String");
            model2code.put("DateTime", "DateTime");
            model2code.put("DateTime2", "DateTime");
            model2code.put("Decimal", "decimal");    		
    	}
    	
    	return model2code;
    }

    public Field()
    {
    }
    
    public String getForeignKeyName()
    {
    	return UUID.randomUUID().toString().replace('-', '_');
    }
    
    public Boolean getIsLast()
    {
        return entity.isLastField(this);
    }	
    
}
