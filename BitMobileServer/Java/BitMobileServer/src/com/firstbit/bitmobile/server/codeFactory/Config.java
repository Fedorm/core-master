package com.firstbit.bitmobile.server.codeFactory;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

public class Config 
{
	private List<Entity> entities;
    private HashMap<String, List<Constant>> constants;	
    private HashMap<String, Parameter> globalParameters;
    private String solutionName;
    private ConfigVersion configVersion;
    
    public Config(ConfigVersion version, String solutionName, List<Entity> entities, HashMap<String, Parameter> globalParameters, HashMap<String, List<Constant>> constants)
    {
        this.configVersion = version;
        this.solutionName = solutionName;
        this.entities = entities;
        this.constants = constants;
        this.globalParameters = globalParameters;

        /*
        for(Entity entity:entities)
        {
            entity.BuildSyncFilter(this);
        }
        */
    }

	public List<Entity> getEntities() {
		return entities;
	}

	public HashMap<String, List<Constant>> getConstants()
	{
		return constants;
	}
	
	public HashMap<String, Parameter> getGlobalParameters()
	{
		return globalParameters;
	}

	public ConfigVersion getConfigVersion() {
		return configVersion;
	}
    
    public String getScope()
    {
        return "DefaultScope";
    }

    public String getServerScope()
    {
        return "DefaultScope";
    }   

	public String getSolutionName() {
		return solutionName;
	}
    
    public String getDatabaseName()
    {
    	return this.getSolutionName();
    }
    
    public HashMap<String, List<Entity>> getEntitiesBySchema()
    {
    	HashMap<String, List<Entity>> result = new HashMap<String, List<Entity>>();
        for(Entity e:entities)
        {
            if (!result.containsKey(e.getSchema()))
                result.put(e.getSchema(), new ArrayList<Entity>());
            result.get(e.getSchema()).add(e);
        }
        return result;
    }	   
}
