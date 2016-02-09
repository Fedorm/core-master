import org.eclipse.debug.core.DebugException;
import org.eclipse.debug.core.model.IBreakpoint;
import org.eclipse.debug.core.model.IDebugTarget;
import org.eclipse.debug.core.model.IStackFrame;
import org.eclipse.debug.core.model.IThread;


public class BMThread extends BMDebugElement implements IThread,
		IBMEventListener {

	public BMThread(IDebugTarget target) {
		super(target);
		System.out.println("thread created");
	}

	private int fLineNumber=0;
	private String fFilePath;
	private BMLineBreakpoint fBreakpoint;

	void start() {
	    fireCreationEvent();
	    ((BMDebugTarget)getDebugTarget()).addEventListener(this);
	}
	
	void exit() { 
		((BMDebugTarget)getDebugTarget()).removeEventListener(this);
		fireTerminateEvent();
	}
	
	public boolean canResume() {
		return getDebugTarget().canResume();
	}

	public boolean canSuspend() {
		return getDebugTarget().canSuspend();
	}

	public boolean isSuspended() {
		return getDebugTarget().isSuspended();
	}

	public void resume() throws DebugException {
		getDebugTarget().resume();
	}

	public void suspend() throws DebugException {
		getDebugTarget().suspend();
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

	public boolean canTerminate() {
		return getDebugTarget().canTerminate();
	}

	public boolean isTerminated() {
		return getDebugTarget().isTerminated();
	}

	public void terminate() throws DebugException {
		getDebugTarget().terminate();
	}

	public void handleEvent(BMEvent event) {
		((BMDebugTarget)getDebugTarget()).handleEvent(event);
	}

	public IStackFrame[] getStackFrames() throws DebugException {
		System.out.println("getstackframes "+isSuspended());
		if (isSuspended()) {
			IStackFrame[] result=new IStackFrame[1];
			System.out.println("thread creating frame "+fFilePath+fLineNumber);
			result[0]=new BMStackFrame(this,fFilePath,fLineNumber);
			return result;
		}
		return new IStackFrame[0];
	}

	public boolean hasStackFrames() throws DebugException {
		System.out.println("hasframes?" +isSuspended());
		return isSuspended();
	}

	public int getPriority() throws DebugException {
		return 0;
	}

	public IStackFrame getTopStackFrame() throws DebugException {
		System.out.println("gettopframe");
		IStackFrame[] frames = getStackFrames();
		if (frames.length > 0) {
			return frames[0];
		}
		return null;
	}

	public String getName() throws DebugException {
		return "Main thread";
	}

	public synchronized IBreakpoint[] getBreakpoints() {
		if (fBreakpoint == null) {
			return new IBreakpoint[0];
		}
		return new IBreakpoint[]{fBreakpoint};
	}

	public synchronized void suspendedByBP(String filePath, int lineNumber) {
		fFilePath=filePath;
		fLineNumber=lineNumber;
		System.out.println("suspended by bp "+fFilePath+fLineNumber);
		
		try {
			suspend();
		} catch (DebugException e) {
			e.printStackTrace();
		}
	}

}
