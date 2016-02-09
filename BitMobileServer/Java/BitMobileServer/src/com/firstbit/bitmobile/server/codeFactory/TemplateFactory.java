package com.firstbit.bitmobile.server.codeFactory;

import java.io.InputStream;
import org.stringtemplate.v4.ST;

import com.firstbit.bitmobile.server.IRequestContext;
import com.firstbit.bitmobile.utils.StringUtils;

public class TemplateFactory 
{
	private final static String templatesPath = "/WEB-INF/templates/database/";
	
	public static String GetTemplate(Config config, String name, IRequestContext ctx) throws Exception
	{
		InputStream is = ctx.getServletContext().getResourceAsStream(String.format("%s%s", templatesPath, name));
		
		ST template = new ST(StringUtils.getStreamContents(is) , '$', '$');
		template.add("cfg", config);
		String result = template.render();
		return result;	
	}	
}
