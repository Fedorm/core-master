package com.firstbit.bitmobile.server.codeFactory;

import java.sql.*;

import com.firstbit.bitmobile.server.dao.ConnectionFactory;
import com.firstbit.bitmobile.utils.StringUtils;

public class DatabaseFactory 
{
    private String connectionString;

    public DatabaseFactory(String connectionString) throws SQLException
    {
        this.connectionString = connectionString;
    }

    public void RunScript(String script) throws SQLException
    {
        Connection conn = ConnectionFactory.getConnection(connectionString);
    	Statement cmd = conn.createStatement();
        cmd.setQueryTimeout(180);
        try
        {
            String[] commands = script.split("###");
            for(String command:commands)
            {
                String s = command.replace("\r\n\r\n", "");
                if(!StringUtils.isNullOrEmpty(s) && !s.equals("\r\n"))
                {
                	cmd.addBatch(s);
                	try
                	{
                		cmd.execute(s);
                	}
                	catch(Exception ex)
                	{
                		throw new SQLException(s);
                	}
                }
            }
            cmd.executeBatch();
            cmd.close();
        }
        catch (Exception e)
        {
            throw e;
        }
    }    
}
