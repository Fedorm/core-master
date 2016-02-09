package com.firstbit.bitmobile.server.admin.core;

import java.sql.SQLException;
import java.text.ParseException;
import java.util.HashMap;

public interface IEntityReadCallback 
{
	public void entityRead(Entity row, Object ctx) throws SQLException, ParseException;
	public void regionalSettingsRead(HashMap<String,String> regionalSettings, Object ctx); 
}
