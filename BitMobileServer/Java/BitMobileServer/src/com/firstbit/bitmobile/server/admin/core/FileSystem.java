package com.firstbit.bitmobile.server.admin.core;

import java.io.IOException;
import java.io.InputStream;
import java.util.List;
import java.util.UUID;
import java.util.zip.ZipEntry;
import java.util.zip.ZipInputStream;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;

import org.apache.commons.codec.binary.Base64;
import org.apache.commons.io.IOUtils;
import org.w3c.dom.*;
import org.xml.sax.SAXException;

import com.firstbit.bitmobile.server.filesystem.GcsFileSystem;

public class FileSystem 
{
	private GcsFileSystem fs;
	public FileSystem(String solutionName)
	{
		fs = new GcsFileSystem(solutionName);
	}
	
	public void WriteUploadAdmin(UUID sessionId, InputStream is) throws IOException
	{
		fs.writeFile(String.format("%s%s", getUploadAdminPath(), sessionId.toString()), is);
	}

	public InputStream ReadUploadAdmin(UUID sessionId) throws IOException
	{
		return fs.readFile(String.format("%s%s", getUploadAdminPath(), sessionId.toString()));
	}
	
	public void WriteConfiguration(InputStream is) throws IOException
	{
		fs.writeFile(getConfigurationFile(), is);
	}

	public Document ReadConfiguration() throws IOException
	{
		DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
		DocumentBuilder builder;
		try 
		{
			builder = factory.newDocumentBuilder();
			InputStream is = null;
			try
			{
				is = fs.readFile(getConfigurationFile());
				Document document = builder.parse(is);
				return document;
			}
			finally
			{
				if(is!=null)
					is.close();
			}
		} 
		catch (ParserConfigurationException | SAXException e) 
		{
			throw new IOException(e);
		}
	}

	public void WriteResourcesArchive(InputStream is) throws IOException
	{
		String rootFolder = getResourceFolder();

		fs.deleteFolder(rootFolder);
		
		ZipInputStream zs = new ZipInputStream(is);
		ZipEntry entry = zs.getNextEntry();
		while(entry!=null)
		{				
			String path = String.format("%s/%s", rootFolder, entry.getName().replace('\\', '/'));
			fs.writeFile(path, zs);
			entry = zs.getNextEntry();
		}				
	}
	
	public String ReadFileAsBase64(String fileName) throws IOException
	{
		InputStream is = fs.readFile(fileName);
		byte[] bytes = IOUtils.toByteArray(is);
		return Base64.encodeBase64String(bytes);
	}
	
	public Boolean getResourceFolderExists() throws IOException
	{
		return fs.listFolder(getResourceFolder(), false).size()>0;
	}

	public Boolean getSettingsFileExists() throws IOException
	{
		List<String> list = fs.listFolder(getResourceFolder(), false);
		return list.contains(getSettingsFile());
	}

	public List<String> listFolder(String folder) throws IOException
	{
		return fs.listFolder(folder, false);
	}

	public List<String> listFolderFilesOnly(String folder, Boolean recursive) throws IOException
	{
		return fs.listFolderDirectoriesOnly(folder, recursive);
	}
	
	public List<String> listFolderDirectoriesOnly(String folder, Boolean recursive) throws IOException
	{
		return fs.listFolderDirectoriesOnly(folder, recursive);
	}
	
	public List<String> listFolder(String folder, Boolean recursive) throws IOException
	{
		return fs.listFolder(folder, recursive);
	}
	
	private String getResourceFolder()
	{	
		return "resource";
	}

	public String getDeviceResourceFolder()
	{	
		return String.format("%s/device", getResourceFolder());
	}
	
	private String getSettingsFile()
	{	
		return String.format("%s/settings.xml", getResourceFolder());
	}
	
	private String getConfigurationFile()
	{
		return "configuration/configuration.xml";
	}

	private String getUploadAdminPath()
	{
		return "upload/admin/";
	}
	
}

