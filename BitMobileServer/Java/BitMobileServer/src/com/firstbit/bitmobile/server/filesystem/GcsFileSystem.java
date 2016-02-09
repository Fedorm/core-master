package com.firstbit.bitmobile.server.filesystem;

import java.io.BufferedOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.nio.channels.Channels;
import java.util.ArrayList;
import java.util.List;

import com.google.appengine.api.appidentity.AppIdentityService;
import com.google.appengine.api.appidentity.AppIdentityServiceFactory;
import com.google.appengine.tools.cloudstorage.*;

public class GcsFileSystem 
{
	private String solutionName;
	private String bucketName = "";
	private GcsService gcs;
	
	public GcsFileSystem(String solutionName)
	{
		this.solutionName = solutionName;
		this.gcs = GcsServiceFactory.createGcsService();
		AppIdentityService appIdentityService = AppIdentityServiceFactory.getAppIdentityService();
		bucketName = appIdentityService.getDefaultGcsBucketName();
	}
	
	public void writeFile(String fileName, InputStream is) throws IOException
	{
		GcsFilename fn = getGcsName(fileName);		
		GcsFileOptions options = new GcsFileOptions.Builder().acl("public-read").build();		
		GcsOutputChannel outputChannel = gcs.createOrReplace(fn, options);
		
	    // write file out
	    BufferedOutputStream outStream = new BufferedOutputStream(Channels.newOutputStream(outputChannel));
	    
	    byte[] buf = new byte[4096];
	    int cnt = 0;
	    while((cnt = is.read(buf, 0, 4096)) > 0)
	    {
	        outStream.write(buf, 0, cnt);
	    }
	    outStream.close();
	    outputChannel.close();
	}
	
	public InputStream readFile(String fileName) throws IOException
	{
		GcsFilename fn = getGcsName(fileName);
		GcsInputChannel channel = gcs.openReadChannel(fn, 0);
		return Channels.newInputStream(channel);
	}
	
	public void deleteFolder(String folderName) throws IOException 
	{
	    ListResult list = gcs.list(bucketName, new ListOptions.Builder().setPrefix(getFolderName(folderName)).setRecursive(true).build());

		while(list.hasNext())
		{
			ListItem item = list.next();
			gcs.delete(new GcsFilename(bucketName, item.getName()));
		}
	}	

	
	public List<String> listFolderFilesOnly(String folderName, Boolean recursive) throws IOException
	{
		return listFolderInternal(folderName, recursive, false, true);
	}

	public List<String> listFolderDirectoriesOnly(String folderName, Boolean recursive) throws IOException
	{
		return listFolderInternal(folderName, recursive, true, false);
	}
	
	public List<String> listFolder(String folderName, Boolean recursive) throws IOException
	{
		return listFolderInternal(folderName, recursive, false, false);
	}
	
	private List<String> listFolderInternal(String folderName, Boolean recursive, Boolean onlyDirectories, Boolean onlyFiles) throws IOException 
	{
		ArrayList<String> result = new ArrayList<String>();
		
	    ListResult list = gcs.list(bucketName, new ListOptions.Builder().setPrefix(getFolderName(folderName)).setRecursive(recursive).build());

		while(list.hasNext())
		{
			ListItem item = list.next();
			if(!(onlyDirectories && !item.isDirectory()) && !(onlyFiles && item.isDirectory()))
				result.add(item.getName().substring(solutionName.length() + 1)); //remove solution name from path
		}
		
		return result;
	}	

	public String getFileName(String fileName)
	{
		return String.format("%s/%s", solutionName, fileName);	
	}
	
	private String getFolderName(String fileName)
	{
		return String.format("%s/%s%s", solutionName, fileName, fileName.endsWith("/")?"":"/");	
	}
	
	private GcsFilename getGcsName(String fileName)
	{
		return new GcsFilename(bucketName, String.format("%s/%s", solutionName, fileName));	
	}
}
