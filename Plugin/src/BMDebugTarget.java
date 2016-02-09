import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.InetSocketAddress;
import java.net.MalformedURLException;
import java.net.Socket;
import java.net.URL;
import java.util.ArrayList;
import java.util.Collections;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

import org.eclipse.core.resources.IMarkerDelta;
import org.eclipse.core.runtime.CoreException;
import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.core.runtime.IStatus;
import org.eclipse.core.runtime.Status;
import org.eclipse.core.runtime.jobs.Job;
import org.eclipse.debug.core.DebugEvent;
import org.eclipse.debug.core.DebugException;
import org.eclipse.debug.core.IBreakpointManager;
import org.eclipse.debug.core.IBreakpointManagerListener;
import org.eclipse.debug.core.ILaunch;
import org.eclipse.debug.core.model.IBreakpoint;
import org.eclipse.debug.core.model.IDebugTarget;
import org.eclipse.debug.core.model.ILineBreakpoint;
import org.eclipse.debug.core.model.IMemoryBlock;
import org.eclipse.debug.core.model.IProcess;
import org.eclipse.debug.core.model.IThread;

import com.sun.net.httpserver.Headers;
import com.sun.net.httpserver.HttpExchange;
import com.sun.net.httpserver.HttpHandler;
import com.sun.net.httpserver.HttpServer;


@SuppressWarnings("restriction")
public class BMDebugTarget extends BMDebugElement implements IDebugTarget,
IBreakpointManagerListener, IBMEventListener {

	
	
    class DebugHttpHandler implements HttpHandler {
        public void handle(HttpExchange t) throws IOException {
	
            Headers responseHeaders = t.getResponseHeaders();
            responseHeaders.set("Content-Type", "text/plain");
            t.sendResponseHeaders(200, 0);    	
            OutputStream os = t.getResponseBody();
            os.write("ok".getBytes());
            os.close();
            
            String path = t.getRequestURI().getPath();
            
		    BMEvent event = null;
		    try {
		    	System.out.println("query: " + t.getRequestURI().getQuery());
		        event = BMEvent.parseEvent(path,t.getRequestURI().getQuery());
				Object[] listeners = fEventListeners.toArray();
				for (int i = 0; i < listeners.length; i++) {
					((IBMEventListener)listeners[i]).handleEvent(event);	
				}
		    }
		    catch (IllegalArgumentException e) {
		        BitMobilePlugin.getDefault().getLog().log(
		            new Status (IStatus.ERROR, "Bitmobile", "Error parsing PDA event", e));
		    }
        	
        }
    }	
    
	private EventDispatchJob fEventDispatch;
	private Socket fEventSocket;
	private BufferedReader fEventReader;
    
	class EventDispatchJob extends Job {
		
		public EventDispatchJob() {
			super("BM remote console Dispatch");
			setSystem(true);
		}

		protected IStatus run(IProgressMonitor monitor) {
			String message = "";
			while (!isTerminated() && message != null) {
				try {
					message = fEventReader.readLine();
					if (message != null) {
					    BitMobilePlugin.printToConsole(message);
					}
				} catch (IOException e) {
				}
			}
			return Status.OK_STATUS;
		}
		
	}
    
    
    String fDeviceAddr;
    int fDebugPort;
    
	public BMDebugTarget(ILaunch launch,String deviceAddr,int debugPort) throws DebugException {
		super(null);
		fDeviceAddr=deviceAddr;
		fDebugPort=debugPort;
		System.out.println("target cons");
		fLaunch = launch;
		addEventListener(this);

		try {
			server = HttpServer.create(new InetSocketAddress(8082), 0);
	        server.createContext("/debugger", new DebugHttpHandler());
	        server.setExecutor(null); 
	        server.start();		
		} catch (IOException e) {
			e.printStackTrace();
		}

		try {
			Thread.sleep(1000);
		} catch (InterruptedException e) {
			
		}

		try {
			fEventSocket = new Socket(deviceAddr, 8081);
			fEventReader = new BufferedReader(new InputStreamReader(fEventSocket.getInputStream()));
			fEventDispatch = new EventDispatchJob();
			fEventDispatch.schedule();
		} catch (Exception e) {
			e.printStackTrace();
		}

		
		
		IBreakpointManager breakpointManager = getBreakpointManager();
        breakpointManager.addBreakpointListener(this);
		breakpointManager.addBreakpointManagerListener(this);
//		sendCommand(new BMRunCommand());
	}

	private ILaunch fLaunch;
	
	private HttpServer server;
	
	private boolean fTerminated=false; 
	private boolean fSuspended=false; 
	
	private BMThread fThread=null;
	private List<IBMEventListener> fEventListeners = Collections.synchronizedList(new ArrayList<IBMEventListener>());
	private Map<Integer, BMThread> fThreads = Collections.synchronizedMap(new LinkedHashMap<Integer, BMThread>());

	public void addEventListener(IBMEventListener listener) {
	    synchronized(fEventListeners) {
			System.out.println("addeventlistener "+listener.toString());
    		if (!fEventListeners.contains(listener)) {
    			fEventListeners.add(listener);
    		}
	    }
	}
	
	public void removeEventListener(IBMEventListener listener) {
		System.out.println("remeventlistener "+listener.toString());
		fEventListeners.remove(listener);
	}
	
	public String readBigStringIn(BufferedReader buffIn) {
        StringBuilder b = new StringBuilder();

        try {
            String line = buffIn.readLine();
            while (line != null) {
            	if (b.length()>0) {
            		b.append("\r\n");
            	}
                b.append(line);
                line = buffIn.readLine();
            }
        }
        catch(IOException e){};

        return b.toString();
}	
	
	private String sendRequest(String request) throws DebugException {
		System.out.println("sendrequest "+request);
		String deviceURL="http://"+fDeviceAddr+":"+fDebugPort+"/debugger/"+request;
		try {
			HttpURLConnection con=(HttpURLConnection)(new URL(deviceURL).openConnection());
			con.setRequestMethod("GET");
			BufferedReader rd=new BufferedReader(new InputStreamReader(con.getInputStream()));
			String result=readBigStringIn(rd); //rd.readLine();
			return result;
		} catch (MalformedURLException e) {
		} catch (IOException e) {
		}
		return null;
	}  
	
	public BMCommandResult sendCommand(BMCommand command) throws DebugException {
		String response = sendRequest(command.getRequest());
		System.out.println("command result "+response);
	    return command.createResult(response);
	}	
	public IDebugTarget getDebugTarget() {
		System.out.println("target requested "+this.toString());
		return this;
	}

	public ILaunch getLaunch() {
		return fLaunch;
	}

	public boolean canTerminate() {
		return !fTerminated;
	}

	public synchronized boolean isTerminated() {
		return fTerminated;
	}

	public synchronized void terminate() throws DebugException {
		System.out.println("target.terminate()");
		if (fThread!=null) { 
			fThread.exit();
		}
		server.stop(0);
		fTerminated=true;
		System.setProperty("BitMobile.isDebugging", "false");
		System.setProperty("BitMobile.isSuspended", "false");
	    sendCommand(new BMTerminateCommand());
		fThread=null;
		fThreads.clear();
		IBreakpointManager breakpointManager = getBreakpointManager();
        breakpointManager.removeBreakpointListener(this);
		breakpointManager.removeBreakpointManagerListener(this);
		fireTerminateEvent();
		removeEventListener(this);

	}

	public boolean canResume() {
		return fSuspended && !fTerminated;
	}

	@Override
	public boolean canSuspend() {
		return !fSuspended && !fTerminated;
//		return false;
	}

	@Override
	public synchronized boolean isSuspended() {
		return fSuspended;
	}

	@Override
	public void resume() throws DebugException {
		System.out.println("target.resume()");
	    fSuspended=false;
		System.setProperty("BitMobile.isSuspended", "false");
	    sendCommand(new BMResumeCommand());
	    fThread.fireResumeEvent(DebugEvent.CLIENT_REQUEST);
	}

	@Override
	public synchronized void suspend() throws DebugException {
		System.out.println("target.suspend()");
	    fSuspended=true;
		System.setProperty("BitMobile.isSuspended", "true");
	    fThread.fireSuspendEvent(DebugEvent.BREAKPOINT);
	}

	@Override
	public void breakpointAdded(IBreakpoint breakpoint) {
		System.out.println("target.breakpointadded()");
		try {
			if ((breakpoint.isEnabled() && getBreakpointManager().isEnabled()) || !breakpoint.isRegistered()) {
				BMLineBreakpoint bmBreakpoint = CastBreakpoint(breakpoint);					
			    bmBreakpoint.install(this);
			}
		} catch (CoreException e) {
		}
	}

	@Override
	public void breakpointRemoved(IBreakpoint breakpoint, IMarkerDelta delta) {
		System.out.println("target.breakpointremoved()");
		try {
		    BMLineBreakpoint bmBreakpoint = CastBreakpoint(breakpoint);
			bmBreakpoint.remove(this);
		} catch (CoreException e) {
		}
	}

	@Override
	public void breakpointChanged(IBreakpoint breakpoint, IMarkerDelta delta) {
		try {
			if (breakpoint.isEnabled() && getBreakpointManager().isEnabled()) {
				breakpointAdded(breakpoint);
			} else {
				breakpointRemoved(breakpoint, null);
			}
		} catch (CoreException e) {
		}
	}

	public boolean canDisconnect() {
		return false;
	}

	public void disconnect() throws DebugException {
	}

	public boolean isDisconnected() {
		return false;
	}

	public boolean supportsStorageRetrieval() {
		return false;
	}

	public IMemoryBlock getMemoryBlock(long startAddress, long length)
			throws DebugException {
		return null;
	}


	public IProcess getProcess() {
		return null;
	}

	public IThread[] getThreads() throws DebugException {
	/*	IThread[] result=new BMThread[1];
		result[0]=fThread;
		System.out.println("getthreads "+result.toString());
        return result;*/
	    synchronized (fThreads) {
	        return (IThread[])fThreads.values().toArray(new IThread[fThreads.size()]);
	    }
	}

	public boolean hasThreads() throws DebugException {
		return fThreads.size() > 0;
//		return fThread!=null;
	}

	@Override
	public String getName() throws DebugException {
		return "BitMobile";
	}

	@Override
	public boolean supportsBreakpoint(IBreakpoint breakpoint) {
		return true;
	}

	@Override
	public void breakpointManagerEnablementChanged(boolean enabled) {
		IBreakpoint[] breakpoints = getBreakpointManager().getBreakpoints(getModelIdentifier());
		for (int i = 0; i < breakpoints.length; i++) {
			if (enabled) {
				breakpointAdded(breakpoints[i]);
			} else {
				breakpointRemoved(breakpoints[i], null);
			}
        }
	}

	private void installDeferredBreakpoints() {
		IBreakpoint[] breakpoints = getBreakpointManager().getBreakpoints(getModelIdentifier());
		for (int i = 0; i < breakpoints.length; i++) {
			breakpointAdded(breakpoints[i]);
		}
	}

	public void handleEvent(BMEvent event) {
		System.out.println("target handleevent "+event.toString());
		if (event instanceof BMStartedEvent) {
			fireCreationEvent();
			installDeferredBreakpoints();
		    fThread = new BMThread(this);
		    fThreads.put(new Integer(0), fThread);
		    fThread.start();
		    try {
				sendCommand(new BMResumeCommand());
			} catch (DebugException e) {
				e.printStackTrace();
			}
		}
		else if (event instanceof BMTerminatedEvent) {
			try {
				terminate();
			} catch (DebugException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}
		else if (event instanceof BMSuspendedEvent) {
			if (event.fArgs.indexOf("exception")>=0) {
		    	String bpArg[] = event.fArgs.split("&");
		    	if (bpArg.length<1) 
		    		return;
				String pair[] = bpArg[0].split("=");
				String fileName;
				int lineNo;
				System.out.println("exception event");
				if (pair.length==2) {
					if (pair[0].equals("exception")) {
						String bp[] = pair[1].split(":");
							if (bp.length==2) {
								fileName = bp[0];
								try {
									lineNo=Integer.parseInt(bp[1]);
									getThread().suspendedByBP(fileName, lineNo);
								}
								catch (Exception e) {
								}
								
							}
					}
				}
				
			}
		}
	}

	public BMThread getThread() {
//		return fThread;
		
	    synchronized(fThreads) {
	        if (fThreads.size() > 0) {
	            return (BMThread)fThreads.values().iterator().next();
	        }
	    }
		return null;
		
	}

	private BMLineBreakpoint CastBreakpoint(IBreakpoint breakpoint)
			throws CoreException {
		BMLineBreakpoint bmBreakpoint;
		if(breakpoint.getClass() != BMLineBreakpoint.class){
			int lineNumber = ((ILineBreakpoint)breakpoint).getLineNumber();
			bmBreakpoint = new BMLineBreakpoint(breakpoint.getMarker().getResource(), lineNumber);
		}					
		else
			bmBreakpoint = (BMLineBreakpoint)breakpoint;
		return bmBreakpoint;
	}
}
