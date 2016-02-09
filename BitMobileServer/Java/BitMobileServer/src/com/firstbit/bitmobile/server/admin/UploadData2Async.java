package com.firstbit.bitmobile.server.admin;

import java.io.PrintWriter;
import java.util.UUID;

import com.firstbit.bitmobile.server.IRequestContext;
import com.firstbit.bitmobile.server.IRequestHandler;
import com.firstbit.bitmobile.server.admin.core.FileSystem;
import com.firstbit.bitmobile.server.queue.TaskQueue;

public class UploadData2Async implements IRequestHandler
{
	@Override
	public void process(IRequestContext ctx) throws Exception
	{
		String solutionName = ctx.getSolutionName();
		UUID sessionId = UUID.randomUUID();
		
		FileSystem fs = new FileSystem(solutionName);
		fs.WriteUploadAdmin(sessionId, ctx.getInputStream());
		
		TaskQueue.addUploadAdminTask(solutionName, sessionId, false);
		
		PrintWriter wr = new PrintWriter(ctx.getOutputStream());
		wr.print(sessionId.toString());
		wr.flush();	
	}
}
