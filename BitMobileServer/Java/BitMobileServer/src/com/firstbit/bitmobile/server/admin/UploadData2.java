package com.firstbit.bitmobile.server.admin;

import java.io.InputStream;
import java.io.PrintWriter;

import com.firstbit.bitmobile.server.IRequestContext;
import com.firstbit.bitmobile.server.IRequestHandler;
import com.firstbit.bitmobile.server.admin.core.DataUploader2;
import com.firstbit.bitmobile.server.codeFactory.Solution;

public class UploadData2  implements IRequestHandler
{
	@Override
	public void process(IRequestContext ctx) throws Exception {
		doWork(ctx, ctx.getSolutionName(), ctx.getInputStream());
		
		PrintWriter wr = new PrintWriter(ctx.getOutputStream());
		wr.print("ok");
		wr.flush();		
	}
	
	private void doWork(IRequestContext ctx, String solutionName, InputStream is) throws Exception
	{
		Solution solution = new Solution(solutionName);
		new DataUploader2(null, 0).UploadData(solution, is, false);
	}	
}
