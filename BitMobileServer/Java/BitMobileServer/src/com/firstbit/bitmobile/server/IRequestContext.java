package com.firstbit.bitmobile.server;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

import javax.servlet.ServletContext;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.firstbit.bitmobile.server.dao.IDbFacade;

public interface IRequestContext 
{
	public HttpServletRequest getContext();
	public void setContext(HttpServletRequest value);
	public ServletContext getServletContext();
	public void setServletContext(ServletContext ctx);
	public String getSolutionName();
	public void setSolutionName(String value);
	public InputStream getInputStream() throws IOException;
	public void setInputStream(InputStream value);
	public OutputStream getOutputStream();
	public void setOutputStream(OutputStream value);
	public void setDbFacade(IDbFacade value);
	public IDbFacade getDbFacade();
	public HttpServletResponse getResponse();
	public void setResponse(HttpServletResponse value);
	
}
