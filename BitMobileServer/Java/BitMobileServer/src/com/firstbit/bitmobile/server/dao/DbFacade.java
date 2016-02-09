package com.firstbit.bitmobile.server.dao;

public class DbFacade implements IDbFacade
{
	private String connectionString;
	
	@Override
	public void setConnectionString(String value) {
		this.connectionString = value;
	}

	@Override
	public String getConnectionString() {
		return this.connectionString;
	}
}
