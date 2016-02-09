import org.eclipse.debug.core.DebugException;
import org.eclipse.debug.core.model.IRegisterGroup;
import org.eclipse.debug.core.model.IStackFrame;
import org.eclipse.debug.core.model.IThread;
import org.eclipse.debug.core.model.IVariable;


public class BMStackFrame extends BMDebugElement implements IStackFrame {

	private BMThread fThread;
	private int fLineNumber=0;
	private String fFilePath;
	
	public BMStackFrame(BMThread thread,String filePath,int lineNumber) {
		super(thread.getDebugTarget());
		fThread=thread;
		fFilePath=filePath;
		fLineNumber=lineNumber;
		System.out.println("frame created "+fFilePath+" "+fLineNumber);
	}
	
	public boolean canStepInto() {
		return false;
	}

	public boolean canStepOver() {
		return false;
	}

	public boolean canStepReturn() {
		return false;
	}

	public boolean isStepping() {
		return false;
	}

	public void stepInto() throws DebugException {
	}

	public void stepOver() throws DebugException {
	}

	public void stepReturn() throws DebugException {
	}

	@Override
	public boolean canResume() {
		
		return getThread().canResume();
	}

	@Override
	public boolean canSuspend() {
		return getThread().canSuspend();
	}

	public boolean isSuspended() {
		return getThread().isSuspended();
	}

	public void resume() throws DebugException {
		getThread().resume();

	}

	@Override
	public void suspend() throws DebugException {
		getThread().suspend();
	}

	@Override
	public boolean canTerminate() {
		return getThread().canTerminate();
	}

	public boolean isTerminated() {
		return getThread().isTerminated();
	}

	public void terminate() throws DebugException {
		getThread().terminate();
	}

	public IThread getThread() {
		return fThread;
	}

	public IVariable[] getVariables() throws DebugException {
		return new IVariable[0];
	}

	public boolean hasVariables() throws DebugException {
		return false;
	}

	public int getLineNumber() throws DebugException {
		// TODO Auto-generated method stub
		return fLineNumber;
	}

	public int getCharStart() throws DebugException {
		// TODO Auto-generated method stub
		return -1;
	}

	public int getCharEnd() throws DebugException {
		// TODO Auto-generated method stub
		return -1;
	}

	public String getName() throws DebugException {
		return fFilePath;
	}

	public IRegisterGroup[] getRegisterGroups() throws DebugException {
		return null;
	}

	public boolean hasRegisterGroups() throws DebugException {
		return false;
	}

}
