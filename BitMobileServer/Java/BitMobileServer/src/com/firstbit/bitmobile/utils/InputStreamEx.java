package com.firstbit.bitmobile.utils;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;

public class InputStreamEx 
{
	private InputStream is;
	byte[] bytes = null;
	
	public InputStreamEx(InputStream is)
	{
		this.is = is;	
	}
	
	public InputStream getInputStream() throws IOException
	{
		if(bytes == null)
		{
			ByteArrayOutputStream baos = new ByteArrayOutputStream();
			org.apache.commons.io.IOUtils.copy(is, baos);	
			bytes = baos.toByteArray();
		}
		return new ByteArrayInputStream(bytes);
	}
	
}
