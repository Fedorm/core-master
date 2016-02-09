package com.firstbit.bitmobile.server;

import java.io.IOException;
import java.io.InputStream;
import java.lang.reflect.Constructor;
import java.util.Enumeration;
import java.util.zip.GZIPInputStream;

import javax.servlet.http.*;

import com.firstbit.bitmobile.server.dao.DbFacade;

@SuppressWarnings("serial")
public class BitMobileServerServlet extends HttpServlet {
	
	public void doPost(HttpServletRequest req, HttpServletResponse resp) throws IOException
	{
		doWork(req, resp);		
	}

	public void doGet(HttpServletRequest req, HttpServletResponse resp) throws IOException 
	{
		doWork(req, resp);				
	}
	
	public void doWork(HttpServletRequest req, HttpServletResponse resp) throws IOException 
	{
		resp.setContentType("text/plain");

		String arr[] = req.getRequestURI().split(";"); //jsession issue
		arr = arr[0].split("/");
		
		String solutionName = null;
		
		String className = "com.firstbit.bitmobile.server";
		for(String s:arr)
		{
			if(s.length()>0)
			{
				if(solutionName==null)
					solutionName = s;
				else
					className = className + "." + s;
			}
		}
		
		Object instance = null;
		try
		{
			Class<?> cls = Class.forName(className);
			Constructor<?> ct = cls.getConstructor((Class[])null);
			instance = ct.newInstance((Object[])null);		
		}
		catch(Exception e)
		{
			throw new IOException(e.getMessage());
		}
		
		DbFacade facade = new DbFacade();
		facade.setConnectionString(this.getServletConfig().getInitParameter("connectionString"));
		
		RequestContext ctx = new RequestContext();
		ctx.setContext(req);
		ctx.setResponse(resp);
		ctx.setSolutionName(solutionName);
		ctx.setInputStream(getInputStream(req));
		ctx.setOutputStream(resp.getOutputStream());
		ctx.setDbFacade(facade);
		ctx.setServletContext(this.getServletContext());
		
		try
		{
			((IRequestHandler)instance).process(ctx);
		}
		catch(UnsupportedOperationException e1)
		{
			resp.sendError(HttpServletResponse.SC_METHOD_NOT_ALLOWED);
		}
		catch(Exception e)
		{
			throw new IOException(e.getMessage());
		}			
	}

	@SuppressWarnings("rawtypes")
	private InputStream getInputStream(HttpServletRequest request) throws IOException
	{
		InputStream is = request.getInputStream();
		if(is == null)
			return null;
		
		Enumeration headerNames = request.getHeaderNames();
		while (headerNames.hasMoreElements()) {
			String key = (String) headerNames.nextElement();
			String value = request.getHeader(key);
			if(key.equalsIgnoreCase("Content-Encoding") && value.equalsIgnoreCase("gzip"))
				return new GZIPInputStream(is);
		}		
		return is;
	}
	
}
