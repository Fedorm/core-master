import java.util.ArrayList;

import org.eclipse.debug.core.DebugException;
import org.eclipse.debug.core.model.IValue;
import org.eclipse.debug.core.model.IVariable;


public class BMValue extends BMDebugElement implements IValue {
	
	private String fValue;
	
	private BMVariable fVariable;
	private IVariable []fVariables=null;

	public BMValue(String value) {
		super(null);
		fValue=value;
		fVariable=null;
	}

	public BMValue(BMVariable variable,String value) {
		super(null);
		fValue=value;
		fVariable=variable;
		try {
			fVariable.setValue(this);
		} catch (DebugException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
	
	
	public String getReferenceTypeName() throws DebugException {
		try {
			Integer.parseInt(fValue);
		} catch (NumberFormatException e) {
			return "text";
		}
		return "integer";
	}

	public String getValueString() throws DebugException {
		return fValue;
	}

	public boolean isAllocated() throws DebugException {
		return true;
	}

	public IVariable[] getVariables() throws DebugException {
		
		if (fVariable!=null) {
			if (fVariables!=null) 
				return fVariables;
		    BMGetFieldsResult result =  
		    		(BMGetFieldsResult) ((BMDebugTarget)fVariable.getDebugTarget()).sendCommand(
		        new BMGetFieldsCommand(fVariable.getName()));
		    
//		    IVariable[] children = new IVariable[result.fValues.length];
		    
		    		    ArrayList<IVariable> childrenList=new ArrayList<IVariable>();
		    
		    for(int i = 0; i < result.fValues.length; i++) {
		    	String []pair=result.fValues[i].split(":");
		    	if (pair.length==2) {
		    		childrenList.add(new BMVariable(((BMDebugTarget)fVariable.getDebugTarget()), fVariable.getName()+"."+pair[0],pair[1]));
		    		
		    	}
		    }
			return (IVariable[])childrenList.toArray(new IVariable[childrenList.size()]);
		}
		else 
			return new IVariable[0];

	}

	public boolean hasVariables() throws DebugException {
		if (fVariable!=null) {
			
			if (fVariables==null) 
				fVariables=getVariables();
			if (getVariables().length != 0) {
				return true;
			}
		}
		return false;
	}

}
