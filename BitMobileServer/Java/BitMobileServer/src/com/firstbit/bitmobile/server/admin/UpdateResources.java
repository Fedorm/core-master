package com.firstbit.bitmobile.server.admin;

import java.io.PrintWriter;

import com.firstbit.bitmobile.server.IRequestContext;
import com.firstbit.bitmobile.server.IRequestHandler;
import com.firstbit.bitmobile.server.admin.core.FileSystem;
import com.firstbit.bitmobile.server.codeFactory.Builder;
import com.firstbit.bitmobile.server.codeFactory.Solution;

public class UpdateResources implements IRequestHandler
{
	@Override
	public void process(IRequestContext ctx) throws Exception
	{
		Solution solution = new Solution(ctx.getSolutionName());
		new Builder(ctx).Build(new FileSystem(solution.getName()).ReadConfiguration(), solution, true);
		
		PrintWriter wr = new PrintWriter(ctx.getOutputStream());
		wr.print("ok");
		wr.flush();
	}
}
