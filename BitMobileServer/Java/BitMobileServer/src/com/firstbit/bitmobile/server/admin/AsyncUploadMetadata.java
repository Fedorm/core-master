package com.firstbit.bitmobile.server.admin;

import java.io.PrintWriter;
import java.util.UUID;

import com.firstbit.bitmobile.server.IRequestContext;
import com.firstbit.bitmobile.server.IRequestHandler;
import com.firstbit.bitmobile.server.admin.core.FileSystem;
import com.firstbit.bitmobile.server.codeFactory.Builder;
import com.firstbit.bitmobile.server.codeFactory.Solution;

public class AsyncUploadMetadata implements IRequestHandler
{
	@Override
	public void process(IRequestContext ctx) throws Exception 
	{
		String solutionName = ctx.getSolutionName();
		UUID sessionId = UUID.fromString(ctx.getContext().getParameter("sessionId"));
		
		FileSystem fs = new FileSystem(solutionName);
		
		Solution solution = new Solution(solutionName);
		new Builder(ctx).Build(fs.ReadConfiguration(), solution, false);
		
		PrintWriter wr = new PrintWriter(ctx.getOutputStream());
		wr.print("ok");
		wr.flush();			
	}
	
}
