package com.firstbit.bitmobile.server.codeFactory;

import java.io.IOException;
import java.sql.*;
import java.text.ParseException;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.UUID;

import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import com.firstbit.bitmobile.server.IRequestContext;
import com.firstbit.bitmobile.server.admin.core.FileSystem;
import com.firstbit.bitmobile.server.dao.CallFactory;
import com.firstbit.bitmobile.server.dao.CallableStatementEx;
import com.firstbit.bitmobile.server.dao.ConnectionFactory;
import com.google.common.collect.Lists;

public class Builder 
{
	private IRequestContext context;
	
	public Builder(IRequestContext ctx)
	{
		this.context = ctx;
	}

    public void Build(Document doc, Solution solution, Boolean onlyResources) throws Exception
    {
    	BuildInternal(doc, solution, onlyResources, false, false);
    }
	
    protected void BuildInternal(Document doc, Solution solution, Boolean onlyResources, Boolean checkTabularSectionKey, Boolean filtersOnly) throws Exception
    {
    	Factory factory = new Factory();
    	
    	HashMap<String, Parameter> globalParameters = factory.getGlobalParameters(doc);
        List<Entity> entities = new ArrayList<Entity>();    
        entities.addAll(factory.GetEntities(doc, checkTabularSectionKey));
        entities.addAll(factory.GetResources());
        entities.addAll(factory.GetAdmin());
        HashMap<String, List<Constant>> constants = factory.GetConstants(doc);
        
        ConfigVersion configVersion = factory.GetVersion(doc);
        Config config = new Config(configVersion, solution.getDatabaseName(), entities, globalParameters, constants);
        
        if (onlyResources)
            BuildResources(config, solution);
        else
        {
            if (filtersOnly)
                BuildFilters(config, solution);
            else
            {
                BuildAll(config, solution);
                InsertConstants(doc, solution);
            }
        }             
    }

    private void BuildAll(Config config, Solution solution) throws Exception
    {
    	context.getContext().setAttribute("cfg", config);

    	String script = "";
    	
    	DatabaseFactory factory = new DatabaseFactory(solution.getDbServer());
    	
    	script = TemplateFactory.GetTemplate(config, "createdatabase", context);
    	factory.RunScript(script);

    	script = TemplateFactory.GetTemplate(config, "core", context);
    	factory.RunScript(script);
    	
    	script = TemplateFactory.GetTemplate(config, "database", context);   	
    	factory.RunScript(script);
    	    	
    	script = TemplateFactory.GetTemplate(config, "databaseadmin", context);
    	factory.RunScript(script);
    	
    	BuildResources(config, solution);
    }
    
    private void BuildResources(Config config, Solution solution) throws IOException, SQLException
    {
    	FileSystem fs = new FileSystem(solution.getName());
    	
        if (fs.getResourceFolderExists())
        {
            //check if settings.xml exists
        	if(!fs.getSettingsFileExists())
                throw new IOException("File settings.xml not found in resource folder");

            CheckMetaVersion(config, solution);

            PopulateResources(solution);
        }
    }

    private void CheckMetaVersion(Config config, Solution solution)
    {
        //TODO
    }    
    
    private void BuildFilters(Config config, Solution solution)
    {
        //TODO    	
    }

    private void InsertConstants(Document doc, Solution solution) throws SQLException
    {
    	Connection c = ConnectionFactory.getConnection(solution.getConnectionString());
    	CallFactory cf = new CallFactory();
    	
        NodeList nodes = XmlHelper.select(doc.getDocumentElement(), "Constants/Entity");
        Node node = null;
         
        c.setAutoCommit(false);
        try
        {
	        for(int i=0;i<nodes.getLength();i++)
	        {
	        	node = nodes.item(i);
				if(node.getNodeType()==Node.ELEMENT_NODE)
				{
					Element eNode = (Element)node;
					String entityName = eNode.getAttribute("Name");
					String[] arr = entityName.split("\\.");
					
					String procName = String.format("%s_%s_adm_insert", arr[0], arr[1]);
					CallableStatementEx cmd = cf.CreateCallStatement(c, procName);
					
			        NodeList rows = XmlHelper.select(eNode, "Row");
			        Node row = null;
			        for(int j=0;j<rows.getLength();j++)
			        {
			        	row = rows.item(j);
						if(row.getNodeType()==Node.ELEMENT_NODE)
						{
							Element r = (Element)row;
							cf.FillParameters(c, cmd, r);
							cmd.getStatement().addBatch();
						}
			        }
					cmd.getStatement().executeBatch();
				}
	        }
	        c.commit();
        }
        catch(SQLException | ParseException e)
        {
        	c.rollback();
        	throw new SQLException(e);
        }
    }    

    private void PopulateResources(Solution solution) throws SQLException, IOException
    {
    	Connection c = ConnectionFactory.getConnection(solution.getConnectionString());
    	CallFactory cf = new CallFactory();
    	
    	FileSystem fs = new FileSystem(solution.getName());
    	
    	c.setAutoCommit(false);
    	try
    	{
    		String rootFolder = fs.getDeviceResourceFolder();
            String schema = "resource";
            for(String app:fs.listFolderDirectoriesOnly(rootFolder,false))
            {
            	String[] arr = app.split("/");
                String appName = arr[arr.length-1];

                for(String dir:fs.listFolderDirectoriesOnly(app,false))
                {
                	String[] arr2 = dir.split("/");
                    String dirName = arr2[arr2.length-1];
                	
                    PreparedStatement cmd = cf.CreateSqlStatement(c, String.format("DELETE FROM %s_%s WHERE Name LIKE ?", schema, dirName));
                    cmd.setString(1, appName + "/%");
                    cmd.execute();
                    cmd.close();

                    CallableStatement cmdInsert = cf.CreateCallStatement(c, String.format("%s_%s_adm_insert", schema, dirName)).getStatement();
                    SaveDirectoryToDB(fs, appName, dirName, dir, cmdInsert);
                    cmdInsert.executeBatch();
                    cmdInsert.close();
                }
            }
    		
    		c.commit();
    	}
        catch(SQLException e)
        {
        	c.rollback();
        	throw new SQLException(e);
        }
    }    
    
    private void SaveDirectoryToDB(FileSystem fs, String appName, String baseDirectory, String directory, CallableStatement cmd) throws IOException, SQLException
    {
        for(String fileName:fs.listFolder(directory, true))
        {
            cmd.setString(1, UUID.randomUUID().toString());
            
            String arr[] = fileName.split("/");
            cmd.setString(2, String.format("%s/%s", appName, arr[arr.length-1]));

            cmd.setString(3, fs.ReadFileAsBase64(fileName));
            
            String[] directories = fileName.split("/");

            ArrayList<String> result = new ArrayList<String>();
            for (int i = directories.length - 2; i >= 0; i--)
            {
                if (directories[i].equals(baseDirectory))
                    break;
                else
                    result.add(directories[i]);
            }
            
            List<String> reversedResult = Lists.reverse(result);
            
            String resultPath = "";
            for(String item:reversedResult)
                resultPath += "\\" + item;
        
            try
            {
            	cmd.setString(4, resultPath);
            
            	cmd.addBatch();
            }
            catch(SQLException e)
            {
            	throw e;
            }
        }
        
        for(String subDir:fs.listFolder(directory))
        {
        	String[] arr = subDir.split("/");
            SaveDirectoryToDB(fs, appName, baseDirectory, arr[arr.length-2], cmd);
        }   
    }   
    
}
