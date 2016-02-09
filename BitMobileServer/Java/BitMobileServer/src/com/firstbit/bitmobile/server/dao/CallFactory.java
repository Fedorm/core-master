package com.firstbit.bitmobile.server.dao;

import java.sql.*;
import java.text.DateFormat;
import java.text.DecimalFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.HashMap;
import java.util.Map.Entry;

import org.w3c.dom.*;

public class CallFactory 
{
	private static HashMap<String, HashMap<String,ParameterInfo>> procColumns = new HashMap<String, HashMap<String,ParameterInfo>>();	
	private static HashMap<String,Integer> dataTypes = new HashMap<String,Integer>();
	private DateFormat dateFormat;
	private DecimalFormat decimalFormat;
	
	static
	{
		dataTypes.put("INT", Types.INTEGER);
		dataTypes.put("VARCHAR", Types.VARCHAR);				
		dataTypes.put("DECIMAL", Types.DECIMAL);				
		dataTypes.put("BOOL", Types.BOOLEAN);				
		dataTypes.put("DATETIME", Types.DATE);				
		dataTypes.put("LONGTEXT", Types.LONGVARCHAR);				
	}

	public CallFactory()
	{
		this.dateFormat = new SimpleDateFormat("MM.dd.yyyy hh:mm:ss");
		this.decimalFormat = new DecimalFormat("#,00000000");		
	}
	
	public CallFactory(DateFormat dateFormat, DecimalFormat decimalFormat)
	{
		this.dateFormat = dateFormat;
		this.decimalFormat = decimalFormat;
	}
	
	private String BuildSqlStatement(Connection c, String procName) throws SQLException
	{
		HashMap<String,ParameterInfo> columns = ObtainProcedureInfo(c, procName);
		String sql = "";
		for(int i=0;i<columns.size();i++)
		{
			if(sql!="")
				sql = sql + ",";
			sql = sql + " ?";
		}
		sql = "CALL " + procName + "(" + sql + ")";
		
		return sql;
	}	
	
	public CallableStatementEx CreateCallStatement(Connection c, String procName) throws SQLException
	{
		String sql = BuildSqlStatement(c, procName);
		return new CallableStatementEx(c.prepareCall(sql), procName);
	}

	public PreparedStatement CreateSqlStatement(Connection c, String sql) throws SQLException
	{
		return c.prepareStatement(sql);
	}
	
	/*
	public static CallableStatement PrepareCommand(Connection c, String procName, Object... params) throws Exception
	{
		HashMap<String,ParameterInfo> columns = ObtainProcedureInfo(c, procName);
		if(columns.size()!=(params==null?0:params.length))
			throw new IllegalArgumentException(procName + " - wrong number of parameters.");
		
		String sql = CreateSqlStatement(c, procName);
		CallableStatement cmd = c.prepareCall(sql);	
		
		for(Entry<String, ParameterInfo> entry:columns.entrySet())
		{
			ParameterInfo pi = entry.getValue();
			Integer paramType = pi.getParamType(dataTypes);
			Integer paramIndex = pi.getParamIndex();
			switch(paramType)
			{
				case Types.INTEGER:
					cmd.setInt(paramIndex, (Integer)params[paramIndex-1]);
					break;
				case Types.VARCHAR:
					cmd.setString(paramIndex, (String)params[paramIndex-1]);
					break;
				case Types.DECIMAL:
					cmd.setDouble(paramIndex, (Double)params[paramIndex-1]);
					break;
				case Types.BOOLEAN:
					cmd.setBoolean(paramIndex, (Boolean)params[paramIndex-1]);
					break;
			}
		}
		return cmd;		
	}
	*/
	
	public HashMap<String,ParameterInfo> ObtainProcedureInfo(Connection c, String procName) throws SQLException
	{
		if(!procColumns.containsKey(procName))
		{
			DatabaseMetaData metaData = c.getMetaData();
			ResultSet rs = metaData.getProcedures(c.getCatalog(), null, procName);
			if(!rs.next())
				throw new SQLException("Procedure " + procName + " does not exist.");
			rs = metaData.getProcedureColumns(c.getCatalog(), null, procName, null);
			HashMap<String,ParameterInfo> columns = new HashMap<String,ParameterInfo>();
			int i=1;
			while(rs.next())
			{
				String colName = rs.getString("COLUMN_NAME");
				String colType = rs.getString("TYPE_NAME");
				Integer colIndex = i;//rs.getInt("ORDINAL_POSITION");
				columns.put(colName, new ParameterInfo(colName,colType,colIndex));
				i++;
			}
			procColumns.put(procName, columns);
		}
		return procColumns.get(procName);
	}
	
	public void FillParameters(Connection c, CallableStatementEx cmd, Element node) throws SQLException, ParseException
	{
		HashMap<String,ParameterInfo> parameters = ObtainProcedureInfo(c, cmd.getProcName());
		
		for(Entry<String, ParameterInfo> entry:parameters.entrySet())
		{
			ParameterInfo pi = entry.getValue();
			
			String paramName = entry.getKey().substring(1); //remove p suffix
			String paramValue = node.getAttribute(paramName);
			Integer paramType = pi.getParamType(dataTypes);
			Integer paramIndex = pi.getParamIndex();
			
			switch(paramType)
			{
				case Types.INTEGER:
					cmd.getStatement().setInt(paramIndex, Integer.parseInt(paramValue));
					break;
				case Types.VARCHAR:
					if(paramValue.equalsIgnoreCase("null") || paramValue.equalsIgnoreCase("00000000-0000-0000-0000-000000000000"))
						cmd.getStatement().setNull(paramIndex, Types.VARCHAR);
					else
						cmd.getStatement().setString(paramIndex, paramValue);
					break;
				case Types.DECIMAL:
					cmd.getStatement().setDouble(paramIndex, decimalFormat.parse(paramValue).doubleValue());
					break;
				case Types.BOOLEAN:
					cmd.getStatement().setBoolean(paramIndex, Boolean.parseBoolean(paramValue));
					break;
				case Types.DATE:
					cmd.getStatement().setDate(paramIndex, (Date) dateFormat.parse(paramValue));
					break;
			}
		}
	}

	public void FillParameters(Connection c, CallableStatementEx cmd, IEntity node) throws SQLException, ParseException
	{
		HashMap<String,ParameterInfo> parameters = ObtainProcedureInfo(c, cmd.getProcName());
		
		for(Entry<String, ParameterInfo> entry:parameters.entrySet())
		{
			ParameterInfo pi = entry.getValue();
			
			String paramName = entry.getKey().substring(1); //remove p suffix
			String paramValue = node.getAttribute(paramName);
			Integer paramType = pi.getParamType(dataTypes);
			Integer paramIndex = pi.getParamIndex();
			
			switch(paramType)
			{
				case Types.INTEGER:
					cmd.getStatement().setInt(paramIndex, Integer.parseInt(paramValue));
					break;
				case Types.VARCHAR:
					if(paramValue.equalsIgnoreCase("null") || paramValue.equalsIgnoreCase("00000000-0000-0000-0000-000000000000"))
						cmd.getStatement().setNull(paramIndex, Types.VARCHAR);
					else
						cmd.getStatement().setString(paramIndex, paramValue);
					break;
				case Types.DECIMAL:
					cmd.getStatement().setDouble(paramIndex, decimalFormat.parse(paramValue).doubleValue());
					break;
				case Types.BOOLEAN:
					cmd.getStatement().setBoolean(paramIndex, Boolean.parseBoolean(paramValue));
					break;
				case Types.DATE:
					cmd.getStatement().setDate(paramIndex, new java.sql.Date(dateFormat.parse(paramValue).getTime()));
					break;
			}
		}
	}

	public void FillParameters(Connection c, CallableStatementEx cmd, Object... params) throws SQLException, ParseException
	{
		HashMap<String,ParameterInfo> parameters = ObtainProcedureInfo(c, cmd.getProcName());
		
		if(parameters.size()!=(params==null?0:params.length))
			throw new IllegalArgumentException(cmd.getProcName() + " - wrong number of parameters.");
		
		
		for(Entry<String, ParameterInfo> entry:parameters.entrySet())
		{
			ParameterInfo pi = entry.getValue();
			Integer paramType = pi.getParamType(dataTypes);
			Integer paramIndex = pi.getParamIndex();
			Object paramValue = params[paramIndex-1];
			
			switch(paramType)
			{
				case Types.INTEGER:
					cmd.getStatement().setInt(paramIndex, (Integer)paramValue);
					break;
				case Types.VARCHAR:
					if(paramValue == null)
						cmd.getStatement().setNull(paramIndex, Types.VARCHAR);
					else
						cmd.getStatement().setString(paramIndex, (String)paramValue);
					break;
				case Types.LONGVARCHAR:
					if(paramValue == null)
						cmd.getStatement().setNull(paramIndex, Types.VARCHAR);
					else
						cmd.getStatement().setString(paramIndex, (String)paramValue);
					break;
				case Types.DECIMAL:
					cmd.getStatement().setDouble(paramIndex, (Double)paramValue);
					break;
				case Types.BOOLEAN:
					cmd.getStatement().setBoolean(paramIndex, (Boolean)paramValue);
					break;
				case Types.DATE:
					throw new SQLException("Date argument is not supported");
			}
		}
	}
	
}
