package com.firstbit.bitmobile.server.guice;

import java.util.logging.Logger;
import com.google.inject.AbstractModule;

public class Module extends AbstractModule 
{
	private static final Logger log = Logger.getLogger(Module.class.getName());
	
	@Override
	protected void configure() {
		log.fine("Setting up bindings for guice...");
	}
}