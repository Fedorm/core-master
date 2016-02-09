package com.firstbit.bitmobile.server.guice;

import java.util.logging.Logger;

import com.firstbit.bitmobile.server.BitMobileServerServlet;
import com.google.inject.Scopes;

public class ServletModule extends com.google.inject.servlet.ServletModule 
{
	private static final Logger log = Logger.getLogger(ServletModule.class.getName());

	@Override 
	protected void configureServlets() {
		log.fine("configuring servlets");
		
		bind(BitMobileServerServlet.class).in(Scopes.SINGLETON);
		serve("/*").with(BitMobileServerServlet.class);
	}
}