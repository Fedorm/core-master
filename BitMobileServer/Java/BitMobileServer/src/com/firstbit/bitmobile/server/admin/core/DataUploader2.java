package com.firstbit.bitmobile.server.admin.core;

import java.sql.*;
import java.text.DecimalFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map.Entry;
import java.io.InputStream;

import org.xml.sax.SAXException;

import com.firstbit.bitmobile.server.codeFactory.Solution;
import com.firstbit.bitmobile.server.dao.CallFactory;
import com.firstbit.bitmobile.server.dao.CallableStatementEx;
import com.firstbit.bitmobile.server.dao.ConnectionFactory;
import com.firstbit.bitmobile.utils.*;

public class DataUploader2 implements IEntityReadCallback
{
    private IProgressCallback progressCallback;
    private int progressStep;
    private int progressCnt = 0;
    
    private CallFactory callFactory;
    	
	public DataUploader2(IProgressCallback callback, int progressStep)
	{
		this.progressCallback = callback;
		this.progressStep = progressStep;
	}

	@Override
	public void regionalSettingsRead(HashMap<String, String> regionalSettings, Object ctx) 
	{
		if(regionalSettings!=null)
			this.SetRegionalSettings(regionalSettings);		
	}
	
	@Override
	public void entityRead(Entity row, Object ctx) throws SQLException, ParseException
	{
		if(row!=null)
		{
			try
			{
				ReadContext context = (ReadContext)ctx;
		        if (UploadRow(row, context.conn, context.sqlCommands, context.fkErrors, context.checkExisting) && progressCallback != null)
		        {
		            progressCnt++;
		            if ((progressCnt % progressStep) == 0 && progressCallback!=null)
		                progressCallback.execute(progressCnt);
		        }
			}
	        catch(SQLException | ParseException e)
	        {
	        	throw e;
	        }
		}
	}    
		
	public void UploadData(Solution solution, InputStream messageBody, Boolean checkExisting) throws SQLException, ParseException, SAXException
	{
		UpdateCurrentCulterInfo();
		
        int progressCnt = 0;
		
		Connection conn = GetConnection(solution);
		conn.setAutoCommit(false);
		
		HashMap<String, CallableStatementEx[]> sqlCommands = new HashMap<String, CallableStatementEx[]>();
		HashMap<Entity, SQLException> fkErrors = new HashMap<Entity, SQLException>();
		
		try
		{
			try
			{
	            if (progressCallback != null)
	                progressCallback.execute(0);
	            
	            FastXmlReader reader = new FastXmlReader();
	            
	            reader.start(messageBody, this, new ReadContext(conn, sqlCommands, fkErrors, checkExisting)); 
	            
                int pass = 0;
                int cnt = 0;
                List<Entity> errorRows = null;
                while (fkErrors.size() > 0)
                {
                    if (pass > 0 && cnt == 0 && fkErrors.size() > 0)
                    {
                    	Iterator<SQLException> err = fkErrors.values().iterator();
                        if (err.hasNext())
                            throw err.next();
                    }

                    cnt = 0;
                    errorRows = new ArrayList<Entity>(fkErrors.keySet());
                    for(Entity row:errorRows)
                    {
                        if (UploadRow(row, conn, sqlCommands, fkErrors, checkExisting))
                        {
                            fkErrors.remove(row);
                            cnt++;

                            if (progressCallback != null)
                            {
                                progressCnt++;
                                if ((progressCnt % progressStep) == 0 && progressCallback!=null)
                                    progressCallback.execute(progressCnt);
                            }
                        }
                    }

                    pass++;
                }
                conn.commit();	            
			}
			catch(Exception e)
			{
                try
                {
                    conn.rollback();
                }
                catch(Exception e2)
                {
                }
                throw e;
			}
		}
		finally
		{
            try
            {
                if (sqlCommands != null)
                {
                    for(Entry<String, CallableStatementEx[]> kvp:sqlCommands.entrySet())
                    {
                    	CallableStatementEx[] arr = kvp.getValue();
                        for (int i = 0; i < arr.length; i++)
                        {
                            if (arr[i] != null)
                            {
                                arr[i].getStatement().close();
                            }
                        }
                    }
                }
            }
            catch(Exception e3)
            {
            }			
		}
		
	}
	    
    private Boolean UploadRow(Entity row, Connection conn, HashMap<String, CallableStatementEx[]> sqlCommands, HashMap<Entity, SQLException> fkErrors, Boolean checkExisting) throws SQLException, ParseException
    {
        String rawEntityName = row.getAttributes().get("_Type");
        int cmdType = Integer.parseInt(row.getAttributes().get("_RS"));
        String[] arr = rawEntityName.split("\\.");
        String entityName = String.format("%s_%s", arr[0], arr[1]);
        CallableStatementEx[] cmds = null;
        
        if (!sqlCommands.containsKey(entityName))
        {
            cmds = new CallableStatementEx[5];
            
            cmds[0] = callFactory.CreateCallStatement(conn, String.format("%s_%s_adm_insert", arr[0], arr[1]));
            cmds[1] = callFactory.CreateCallStatement(conn, String.format("%s_%s_adm_update", arr[0], arr[1]));
            cmds[2] = callFactory.CreateCallStatement(conn, String.format("%s_%s_adm_delete", arr[0], arr[1]));
            cmds[3] = callFactory.CreateCallStatement(conn, String.format("%s_%s_adm_markdelete", arr[0], arr[1]));
            cmds[4] = null;
            sqlCommands.put(entityName, cmds);
            
            if (checkExisting)
            {
            	try
            	{
            		cmds[4] = callFactory.CreateCallStatement(conn, String.format("%s_%s_adm_exists", arr[0], arr[1]));
            		cmds[4].getStatement().registerOutParameter(2, Types.INTEGER);
                }
                catch(Exception any)
                {
                }
            }
        }
        else
        	cmds = sqlCommands.get(entityName);
        
        CallableStatementEx cmd = cmds[cmdType];
        callFactory.FillParameters(conn, cmd, row);
        
        if (checkExisting && cmds[4] != null && (cmdType == 0 || cmdType == 1))
        {
        	cmds[4].getStatement().setString(1, row.getAttributes().get("Id"));   	
        	Object v = cmds[4].ExecuteScalar();
            if (v != null && cmdType == 0) //exists, will do update instead
            {
                cmdType = 1;
                cmd = cmds[cmdType];
                callFactory.FillParameters(conn, cmd, row);
            }
            if (v == null && cmdType == 1) //not exists, will do insert instead
            {
                cmdType = 0;
                cmd = cmds[cmdType];
                callFactory.FillParameters(conn, cmd, row);
            }
        }        
        
        try
        {
            cmd.getStatement().execute();

            if (cmdType == 0)
                row.getAttributes().put("_RS", "1");

            String entityId = row.getAttributes().get("Id");
            
            if (cmdType != 2 && row.hasTabularSections()) //insert, update or markdelete
            {
                for(Entry<String, TabularSection> item:row.getTabularSections().entrySet())
                {
                    TabularSection tSection =  item.getValue();

                    CallableStatementEx childCmd = null;
                    String ts = String.format("%s_%s_%s", arr[0], arr[1], item.getKey());
                    
                    CallableStatementEx[] cmdsc = null;
                    if (!sqlCommands.containsKey(ts))
                    {
                        cmdsc = new CallableStatementEx[7];
                        cmdsc[0] = callFactory.CreateCallStatement(conn, String.format("%s_adm_clear", ts));
                        cmdsc[1] = callFactory.CreateCallStatement(conn, String.format("%s_adm_insert", ts));
                        		
                        if (!StringUtils.isNullOrEmpty(tSection.getKey()))
                        {
                            cmdsc[2] = callFactory.CreateCallStatement(conn, String.format("%s_adm_update", ts));
                            cmdsc[3] = callFactory.CreateCallStatement(conn, String.format("%s_adm_delete", ts));
                            cmdsc[4] = callFactory.CreateCallStatement(conn, String.format("%s_adm_selectkeys", ts));
                        }
                    
                        sqlCommands.put(ts, cmdsc);
                    }
                    cmdsc = sqlCommands.get(ts);
                    
                    if (cmdType > 0) //update or markdelete
                    {
                        if (StringUtils.isNullOrEmpty(tSection.getKey()) || item.getValue().getEntities().size() == 0)
                        {
                            //remove all
                            childCmd = cmdsc[0];
                            childCmd.getStatement().setString("pRef", entityId);
                            childCmd.ExecuteNonQuery();

                            //insert rows
                            if (item.getValue().getEntities().size() > 0)
                                InsertTabularSection(conn, entityId, item.getValue(), cmdsc[1]);
                        }
                        else
                        {
                        	//select existing                        	
                        	childCmd = cmdsc[4];
                        	childCmd.getStatement().setString("pRef", entityId);
                        	ResultSet tbl = childCmd.getStatement().executeQuery();
                        	
                            String[] keyFields = tSection.getKeyFields();
                            HashMap<String, String> keyValues = new HashMap<String, String>();

                            //update rows
                            childCmd = cmdsc[2];
                            int cnt = 0;
                            for(Entity childRow:item.getValue().getEntities())
                            {
                                callFactory.FillParameters(conn, childCmd, childRow);

                                String stringKey = "";
                                for(String kf:keyFields)
                                {
                                	String keyValue = childRow.getAttribute(kf);//  childCmd.getStatement().getString("p" + kf);
                                    stringKey = stringKey + (keyValue == null ? "null" : keyValue.toString()) + "_";
                                }
                                if (!keyValues.containsKey(stringKey))
                                    keyValues.put(stringKey, "");

                                childCmd.getStatement().setString(1, entityId); //pRef
                                childCmd.getStatement().addBatch();
                                cnt++;
                            }
                            if(cnt>0)
                            	childCmd.getStatement().executeBatch();
                            
                            //remove
                            childCmd = cmdsc[3];
                            cnt = 0;
                            while(tbl.next())
                            {
                                String stringKey = "";
                                for(String kf:keyFields)
                                {
                                    childCmd.getStatement().setString("p" + kf, tbl.getString(kf));
                                    Object keyValue = tbl.getObject(kf);
                                    stringKey = stringKey + (keyValue == null ? "null" : keyValue.toString()) + "_";
                                }
                                if (!keyValues.containsKey(stringKey))
                                {
                                    childCmd.getStatement().setString("pRef", entityId);
                                    childCmd.getStatement().addBatch();
                                    cnt++;
                                }                            	
                            }
                            if(cnt>0)
                            	childCmd.getStatement().executeBatch();
                        }
                    }
                    else
                    {
                        //insert rows
                        InsertTabularSection(conn, entityId, item.getValue(), cmdsc[1]);
                    }
                }
            }
            
            return true;
        }
        catch (SQLException e)
        {
            if (e.getMessage().toLowerCase().contains("foreign key") || e.getMessage().toLowerCase().contains("reference constraint"))
            {
                if (!fkErrors.containsKey(row))
                    fkErrors.put(row, new SQLException(String.format("Key violation at object '%s'", row.getAttributes().get("Id"), e)));
                return false;
            }
            else
                throw e;
        }    
    }
                        
    private void InsertTabularSection(Connection conn, String entityId, TabularSection section, CallableStatementEx childCmd) throws SQLException, ParseException
    {
    	if(section.getEntities().size()>0)
    	{
    		int cnt=0;
	        for(Entity childRow:section.getEntities())
	        {
				callFactory.FillParameters(conn, childCmd, childRow);
	        	childCmd.getStatement().setString(1, entityId); //pRef
	            childCmd.getStatement().addBatch();
	            cnt++;
	        }
	        if(cnt>0)
	        	childCmd.getStatement().executeBatch();
    	}
    } 
    
    private Connection GetConnection(Solution solution) throws SQLException
    {
        return ConnectionFactory.getConnection(solution.getConnectionString());
    }	

    private void UpdateCurrentCulterInfo()
	{
    	callFactory = new CallFactory();
	}
	    
    private void SetRegionalSettings(HashMap<String,String> regionalSettings)
    {
    	String ngs = regionalSettings.get("NumberGroupSeparator");    	    	    	
    	String nds = regionalSettings.get("NumberDecimalSeparator");

    	this.callFactory = new CallFactory(new SimpleDateFormat("MM.dd.yyyy hh:mm:ss"), new DecimalFormat(String.format("#%s##0%s00000000", ngs==null?"":ngs, nds==null?".":nds)));
    }
   
    private class ReadContext
    {
    	Connection conn;
		HashMap<String, CallableStatementEx[]> sqlCommands;
		HashMap<Entity, SQLException> fkErrors;
		Boolean checkExisting;
		
		public ReadContext(Connection conn, HashMap<String, CallableStatementEx[]> sqlCommands, HashMap<Entity, SQLException> fkErrors, Boolean checkExisting)
		{
			this.conn = conn;
			this.sqlCommands = sqlCommands;
			this.fkErrors = fkErrors;
			this.checkExisting = checkExisting;
		}
    }
}
