import org.eclipse.core.resources.IFile;
import org.eclipse.core.runtime.Path;
import org.eclipse.debug.core.DebugException;
import org.eclipse.debug.core.model.IBreakpoint;
import org.eclipse.debug.core.model.ILineBreakpoint;
import org.eclipse.debug.core.model.IValue;
import org.eclipse.debug.ui.IDebugModelPresentation;
import org.eclipse.debug.ui.IValueDetailListener;
import org.eclipse.jface.viewers.LabelProvider;
import org.eclipse.ui.IEditorInput;
import org.eclipse.ui.part.FileEditorInput;


/**
 * Renders BM debug elements
 */
public class BMDebugModelPresentation extends LabelProvider implements IDebugModelPresentation {
	/* (non-Javadoc)
	 * @see org.eclipse.debug.ui.IDebugModelPresentation#setAttribute(java.lang.String, java.lang.Object)
	 */
	public void setAttribute(String attribute, Object value) {
	}

	/* (non-Javadoc)
	 * @see org.eclipse.jface.viewers.ILabelProvider#getText(java.lang.Object)
	 */
	public String getText(Object element) {
		if (element instanceof BMDebugTarget) {
			return getTargetText((BMDebugTarget)element);
		} else if (element instanceof BMThread) {
	        return getThreadText((BMThread)element);
	    } else if (element instanceof BMStackFrame) {
	        return getStackFrameText((BMStackFrame)element);
	    }
		return null;
	}
	

	private String getTargetText(BMDebugTarget target) {
	    String label = "";
	    if (target.isTerminated()) {
	    	label = "<terminated>";
	    }
	    
	    Path projectPath;
	    String projectName="";
		try {
			projectPath = new Path(target.getLaunch().getLaunchConfiguration().getAttribute("PROJECT_PATH",""));
			projectName=projectPath.lastSegment();
		} catch (Exception e) {
			e.printStackTrace();
		}
	    return label + projectName;
	}
	
	/**
	 * Returns a label for the given stack frame
	 * 
	 * @param frame a stack frame
	 * @return a label for the given stack frame 
	 */
	private String getStackFrameText(BMStackFrame frame) {
	    try {
	       return frame.getName() + " (line: " + frame.getLineNumber() + ")"; 
	    } catch (DebugException e) {
	    }
	    return null;

	}
	
	/**
	 * Returns a label for the given thread
	 * 
	 * @param thread a thread
	 * @return a label for the given thread
	 */
	private String getThreadText(BMThread thread) {
	    String label="";
		try {
			label = thread.getName();
		} catch (DebugException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	    if (thread.isStepping()) {
	        label += " (stepping)";
	    } else if (thread.isSuspended()) {
	        IBreakpoint[] breakpoints = thread.getBreakpoints();
	        if (breakpoints.length == 0) {
	        		label += " (suspended)";
	        } else {
	            IBreakpoint breakpoint = breakpoints[0]; // there can only be one in BM
	            if (breakpoint instanceof BMLineBreakpoint) {
//	            	BMLineBreakpoint BMBreakpoint = (BMLineBreakpoint) breakpoint;
            		label += " (suspended at line breakpoint)";
	            }
	        }
	    } else if (thread.isTerminated()) {
	        label = "<terminated> " + label;
	    }
	    return label;
	}
	
	/* (non-Javadoc)
	 * @see org.eclipse.debug.ui.IDebugModelPresentation#computeDetail(org.eclipse.debug.core.model.IValue, org.eclipse.debug.ui.IValueDetailListener)
	 */
	public void computeDetail(IValue value, IValueDetailListener listener) {
		String detail = "";
		try {
			detail = value.getValueString();
		} catch (DebugException e) {
		}
		listener.detailComputed(value, detail);
	}
	/* (non-Javadoc)
	 * @see org.eclipse.debug.ui.ISourcePresentation#getEditorInput(java.lang.Object)
	 */
	public IEditorInput getEditorInput(Object element) {
		if (element instanceof IFile) {
			return new FileEditorInput((IFile)element);
		}
		if (element instanceof ILineBreakpoint) {
			return new FileEditorInput((IFile)((ILineBreakpoint)element).getMarker().getResource());
		}
		return null;
	}
	/* (non-Javadoc)
	 * @see org.eclipse.debug.ui.ISourcePresentation#getEditorId(org.eclipse.ui.IEditorInput, java.lang.Object)
	 */
	public String getEditorId(IEditorInput input, Object element) {
		if (element instanceof IFile || element instanceof ILineBreakpoint) {
			return "org.eclipse.wst.jsdt.ui.CompilationUnitEditor";
		}
		return null;
	}
}
