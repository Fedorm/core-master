package com.firstbit.bitmobile.server.codeFactory;

public class ConfigVersion 
{
	private String name;
	private String version;
	
	public String getName() {
		return name;
	}
	public void setName(String name) {
		this.name = name;
	}
	
	public String getVersion() {
		return version;
	}
	public void setVersion(String version) {
		this.version = version;
	}
	
    public String getVersionMasked()
    {
        String[] arr = version.split("\\.");
        String result = "";
        for (int i = 0; i < arr.length - 1; i++)
            result = result + arr[i] + ".";
        return result + "%";
    }	
}
