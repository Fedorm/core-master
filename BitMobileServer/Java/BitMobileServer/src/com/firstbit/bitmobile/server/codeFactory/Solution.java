package com.firstbit.bitmobile.server.codeFactory;

public class Solution 
{
	private String name;
	
	public Solution(String name)
	{
		this.name = name;
	}
	
	public String getName() {
		return name;
	}

	/*
    public String getSolutionFolder()
    {
    	return this.name;
    }	
	
    public String getDeviceResourceFolder()
    {
    	return String.format("%s/resource/device", getSolutionFolder());
    }	
    */
    
    public String getDatabaseName()
    {
    	return name;
    }
    
    public String getDbServer()
    {
    	//return "jdbc:mysql://127.0.0.1:3306?user=root&password=12321";
    	return "jdbc:google:rdbms://127.0.0.1:3306?user=root&password=12321";    	
    }
    
    public String getConnectionString()
    {
    	String[] arr = getDbServer().split("\\?");
    	return String.format("%s/%s?%s", arr[0], getDatabaseName(), arr[1]);
    }

}
