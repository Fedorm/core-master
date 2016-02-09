package com.firstbit.bitmobile.server;

public interface IRequestHandler 
{
	public void process(IRequestContext ctx) throws Exception;
}
