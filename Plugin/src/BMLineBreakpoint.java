import org.eclipse.core.resources.IMarker;
import org.eclipse.core.resources.IResource;
import org.eclipse.core.resources.IWorkspaceRunnable;
import org.eclipse.core.runtime.CoreException;
import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.debug.core.model.IBreakpoint;
import org.eclipse.debug.core.model.LineBreakpoint;


public class BMLineBreakpoint extends LineBreakpoint implements
		IBMEventListener {

	private BMDebugTarget fTarget;

	public BMLineBreakpoint() {
	}
	
	public BMLineBreakpoint(final IResource resource, final int lineNumber) throws CoreException {
		IWorkspaceRunnable runnable = new IWorkspaceRunnable() {
			public void run(IProgressMonitor monitor) throws CoreException {
				IMarker marker = resource.createMarker("BitMobile.markerType.lineBreakpoint");
				setMarker(marker);
				marker.setAttribute(IBreakpoint.ENABLED, Boolean.TRUE);
				marker.setAttribute(IMarker.LINE_NUMBER, lineNumber);
				marker.setAttribute(IBreakpoint.ID, getModelIdentifier());
				marker.setAttribute(IMarker.MESSAGE, "Line Breakpoint: " + resource.getName() + " [line: " + lineNumber + "]");
			}
		};
		run(getMarkerRule(resource), runnable);
	}	
	
    private void notifyThread() throws CoreException {
    	if (fTarget != null) {
			BMThread thread = fTarget.getThread();
			if (thread != null) {
    			thread.suspendedByBP(getFilePath(),getLineNumber());
    		}
    	}
    }
    
    private String getFilePath() {
		IMarker m=getMarker();
		String projectPath=m.getResource().getProject().getLocation().toPortableString();
		String resourcePath=m.getResource().getLocation().toPortableString();
		return resourcePath.replace(projectPath, "");
    }

    public void install(BMDebugTarget target) throws CoreException {
    	fTarget = target;
    	target.addEventListener(this);
    	createRequest(target);
    }

    protected void createRequest(BMDebugTarget target) throws CoreException {
		target.sendCommand(new BMSetBreakpointCommand(getFilePath(),getLineNumber()));
	}
    
    protected void clearRequest(BMDebugTarget target) throws CoreException {
        target.sendCommand(new BMClearBreakpointCommand(getFilePath(),getLineNumber()));
    }
    
    public void remove(BMDebugTarget target) throws CoreException {
    	target.removeEventListener(this);
    	clearRequest(target);
    	fTarget = null;
    	
    }
    
    protected BMDebugTarget getDebugTarget() {
    	return fTarget;
    }
    
	public void handleEvent(BMEvent event) {
		if (event instanceof BMSuspendedEvent) {
	    	String bpArg[] = event.fArgs.split("&");
	    	if (bpArg.length<1) 
	    		return;
			String pair[] = bpArg[0].split("=");
			String fileName;
			int lineNo;
			System.out.println("breakpoint event");
			if (pair.length==2) {
				if (pair[0].equals("breakpoint")) {
					String bp[] = pair[1].split(":");
						if (bp.length==2) {
							fileName = bp[0];
							try {
								lineNo=Integer.parseInt(bp[1]);
								if ((getLineNumber()==lineNo) && (getFilePath().equals(fileName))) {
									notifyThread();
								}
							}
							catch (Exception e) {
							}
							
						}
				}
			}
		}

	}

	public String getModelIdentifier() {
		return BitMobilePlugin.ID_BM_DEBUG_MODEL;
	}

}
