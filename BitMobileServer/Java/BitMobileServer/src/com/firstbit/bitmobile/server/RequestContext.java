package com.firstbit.bitmobile.server;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

import javax.servlet.ServletContext;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.firstbit.bitmobile.server.dao.IDbFacade;

public class RequestContext implements IRequestContext
{
	private ServletContext servletContext;
	private HttpServletRequest context;
	private HttpServletResponse response;
	private String solutionName;
	private InputStream inputStream;
	private OutputStream outpuStream;
	private IDbFacade facade;
	
	@Override
	public OutputStream getOutputStream() {
		return outpuStream;
	}

	@Override
	public void setOutputStream(OutputStream os) {
		outpuStream = os;
	}

	@Override
	public void setDbFacade(IDbFacade value) {
		this.facade = value;
	}

	@Override
	public IDbFacade getDbFacade() {
		return facade;
	}

	@Override
	public InputStream getInputStream() throws IOException
	{
		return this.inputStream;
	}

	@Override
	public void setInputStream(InputStream value) {
		this.inputStream = value;
	}

	@Override
	public String getSolutionName() {
		return this.solutionName;
	}

	@Override
	public void setSolutionName(String value) {
		this.solutionName = value;
	}

	@Override
	public HttpServletRequest getContext() {
		return this.context;
	}

	@Override
	public void setContext(HttpServletRequest value) {
		this.context = value;
	}

	@Override
	public HttpServletResponse getResponse() {
		return this.response;
	}

	@Override
	public void setResponse(HttpServletResponse value) {
		this.response = value;
	}

	@Override
	public ServletContext getServletContext() {
		return this.servletContext;
	}

	@Override
	public void setServletContext(ServletContext ctx) {
		this.servletContext = ctx;
	}

}
