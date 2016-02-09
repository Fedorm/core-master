package com.firstbit.bitmobile.server.dao;

import java.util.HashMap;

public class ParameterInfo
{
	//private String paramName;
	private String paramType;
	private int paramIndex;
	
	public ParameterInfo(String paramName, String paramType, int paramIndex)
	{
		//this.paramName = paramName;
		this.paramType = paramType;
		this.paramIndex = paramIndex;
	}
	
	public int getParamType(HashMap<String,Integer> dataTypes)
	{
		return dataTypes.get(paramType);
	}
	
	public int getParamIndex()
	{
		return paramIndex;
	}
}