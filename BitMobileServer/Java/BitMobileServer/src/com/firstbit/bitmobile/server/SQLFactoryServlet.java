package com.firstbit.bitmobile.server;
import java.io.IOException;

import javax.servlet.RequestDispatcher;
import javax.servlet.ServletException;
import javax.servlet.http.*;

@SuppressWarnings("serial")
public class SQLFactoryServlet extends HttpServlet 
{	
	public void doGet(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException 
	{
		RequestDispatcher rd = req.getRequestDispatcher("/WEB-INF/Templates/Database/CreateDatabase.jsp");
		rd.forward(req, resp);
	}
}
