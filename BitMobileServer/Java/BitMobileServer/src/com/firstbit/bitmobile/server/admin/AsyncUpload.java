package com.firstbit.bitmobile.server.admin;

import java.io.PrintWriter;
import java.util.UUID;

import com.firstbit.bitmobile.server.IRequestContext;
import com.firstbit.bitmobile.server.IRequestHandler;
import com.firstbit.bitmobile.server.admin.core.DataUploader2;
import com.firstbit.bitmobile.server.admin.core.FileSystem;
import com.firstbit.bitmobile.server.codeFactory.Solution;

public class AsyncUpload implements IRequestHandler
{
	@Override
	public void process(IRequestContext ctx) throws Exception 
	{
		String solutionName = ctx.getSolutionName();
		UUID sessionId = UUID.fromString(ctx.getContext().getParameter("sessionId"));
		Boolean checkExisting = ctx.getContext().getParameter("checkExisting").equals("1")?true:false;
		
		FileSystem fs = new FileSystem(solutionName);
		
		Solution solution = new Solution(solutionName);
		new DataUploader2(null, 0).UploadData(solution, fs.ReadUploadAdmin(sessionId), checkExisting);
		
		PrintWriter wr = new PrintWriter(ctx.getOutputStream());
		wr.print("ok");
		wr.flush();			
	}
	
}
