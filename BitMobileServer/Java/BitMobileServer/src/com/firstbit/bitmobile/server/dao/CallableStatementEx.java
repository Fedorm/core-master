package com.firstbit.bitmobile.server.dao;

import java.sql.CallableStatement;
import java.sql.SQLException;

public class CallableStatementEx 
{
	private CallableStatement cmd;
	private String procName;
	
	public CallableStatementEx(CallableStatement cmd, String procName)
	{
		this.cmd = cmd;
		this.procName = procName;
	}
	
	public CallableStatement getStatement()
	{
		return cmd;
	}
	
	public String getProcName()
	{
		return procName;
	}
	
	public Object ExecuteScalar() throws SQLException
	{
		cmd.execute();
		Integer result = cmd.getInt(2);
		return result==1?1:null;
	}
	
	public void ExecuteNonQuery() throws SQLException
	{
		cmd.execute();
	}
}
