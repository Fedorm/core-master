package com.firstbit.bitmobile.server.codeFactory;

import java.util.ArrayList;

@SuppressWarnings("serial")
public class FieldList extends ArrayList<Field>
{
	private Entity entity;
	
	public FieldList(Entity entity)
	{
		this.entity = entity;
	}
	
	public boolean add(Field f)
	{
		Boolean result = super.add(f);
		f.setEntity(entity);
		return result;
	}
}
