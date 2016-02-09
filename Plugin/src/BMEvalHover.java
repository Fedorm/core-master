import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;

import org.eclipse.core.runtime.CoreException;
import org.eclipse.debug.core.DebugPlugin;
import org.eclipse.debug.core.ILaunchConfiguration;
import org.eclipse.debug.core.ILaunchConfigurationType;
import org.eclipse.debug.core.ILaunchManager;
import org.eclipse.jface.text.BadLocationException;
import org.eclipse.jface.text.IDocument;
import org.eclipse.jface.text.IRegion;
import org.eclipse.jface.text.ITextViewer;
import org.eclipse.ui.IEditorPart; 
import org.eclipse.wst.jsdt.internal.ui.text.JavaWordFinder;
import org.eclipse.wst.jsdt.ui.text.java.hover.IJavaEditorTextHover;


public class BMEvalHover implements IJavaEditorTextHover {

	@Override
	public String getHoverInfo(ITextViewer textViewer, IRegion hoverRegion) {
		String isSuspended=System.getProperty("BitMobile.isSuspended");
		if (isSuspended!=null && isSuspended.equals("false")) 
			return null;
        IDocument document = textViewer.getDocument();
        if (document != null) {
          	String variableName;
			try {
				variableName = document.get(hoverRegion.getOffset(), hoverRegion.getLength());
	            ILaunchManager launchManager = DebugPlugin.getDefault().getLaunchManager();
	            ILaunchConfigurationType type = launchManager.getLaunchConfigurationType("Bitmobile.launchType");
	            try {
	                ILaunchConfiguration[] configurations = launchManager.getLaunchConfigurations(type);
	                if (configurations.length>0) { 
	            		String deviceAddr=configurations[0].getAttribute("DEVICE_ADDRESS", "");
	            		String debugPort=configurations[0].getAttribute("DEVICE_DEBUG_PORT", "");
	            		String deviceURL="http://"+deviceAddr+":"+debugPort+"/debugger/evaluate?expression="+variableName;
	            		try {
	            			HttpURLConnection con=(HttpURLConnection)(new URL(deviceURL).openConnection());
	            			con.setRequestMethod("GET");
	            			BufferedReader rd=new BufferedReader(new InputStreamReader(con.getInputStream()));
	            			String result=rd.readLine();
	            			return variableName+"="+result;
	            		} catch (MalformedURLException e) {
	            		} catch (IOException e) {
	            		}
	                }
	            } catch (CoreException e) {
	            }
			} catch (BadLocationException e1) {
			}
        }
        return "evaluation error";  	
	}

    public IRegion getHoverRegion(ITextViewer textViewer, int offset) {
            return JavaWordFinder.findWord(textViewer.getDocument(), offset);
    }

	@Override
	public void setEditor(IEditorPart editor) {
		// TODO Auto-generated method stub

	}

}
