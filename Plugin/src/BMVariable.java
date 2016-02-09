import org.eclipse.debug.core.DebugException;
import org.eclipse.debug.core.model.IValue;
import org.eclipse.debug.core.model.IVariable;


public class BMVariable extends BMDebugElement implements IVariable {

	private String fName;
	private BMValue fValue=null;
	private String fValueResult=null;
	
	public BMVariable(BMDebugTarget target,String name) {
		super(target);
		fName=name;
	}

	public BMVariable(BMDebugTarget target,String name,String result) {
		super(target);
		fValueResult=result;
		fName=name;
	}
	
	
	public void setValue(String expression) throws DebugException {
	}

	public void setValue(IValue value) throws DebugException {
		fValue=(BMValue) value;
	}

	public boolean supportsValueModification() {
		return false;
	}

	public boolean verifyValue(String expression) throws DebugException {
		return false;
	}

	public boolean verifyValue(IValue value) throws DebugException {
		return false;
	}

	public IValue getValue() throws DebugException {
		if (fValue==null) {
			if (fValueResult!=null) 
				fValue=new BMValue(this,fValueResult);
			else
			fValue=new BMValue(this,((BMDebugTarget)getDebugTarget()).sendCommand(new BMEvaluateCommand(fName)).fResponseText);
		}
		return fValue;
	}

	public String getName() throws DebugException {
		return fName;
	}

	public String getReferenceTypeName() throws DebugException {
		return null;
	}

	public boolean hasValueChanged() throws DebugException {
		return false;
	}

}
