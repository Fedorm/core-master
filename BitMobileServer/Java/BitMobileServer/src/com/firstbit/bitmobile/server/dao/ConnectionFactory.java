package com.firstbit.bitmobile.server.dao;

import java.sql.*;

import com.google.appengine.api.rdbms.AppEngineDriver;

public class ConnectionFactory 
{
	private static Boolean driverInitialized = false;
	
	public static Connection getConnection(String connectionString) throws SQLException
	{
		if(!driverInitialized)
		{
			DriverManager.registerDriver(new AppEngineDriver());
			DriverManager.registerDriver(new com.mysql.jdbc.Driver());
			//Class.forName("com.mysql.jdbc.Driver");

			driverInitialized = true;
		}

		Connection c = DriverManager.getConnection(connectionString);			
		c.prepareStatement("SET NAMES utf8").execute();
		return c;
	}
}
