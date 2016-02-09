import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;

import org.eclipse.core.commands.ExecutionEvent;
import org.eclipse.core.commands.ExecutionException;
import org.eclipse.core.commands.IHandler;
import org.eclipse.core.commands.IHandlerListener;
import org.eclipse.core.runtime.CoreException;
import org.eclipse.debug.core.DebugPlugin;
import org.eclipse.debug.core.ILaunchConfiguration;
import org.eclipse.debug.core.ILaunchConfigurationType;
import org.eclipse.debug.core.ILaunchManager;
import org.eclipse.swt.SWT;
import org.eclipse.swt.events.KeyAdapter;
import org.eclipse.swt.events.KeyEvent;
import org.eclipse.swt.events.SelectionAdapter;
import org.eclipse.swt.events.SelectionEvent;
import org.eclipse.swt.graphics.Rectangle;
import org.eclipse.swt.layout.GridData;
import org.eclipse.swt.layout.GridLayout;
import org.eclipse.swt.widgets.Button;
import org.eclipse.swt.widgets.Display;
import org.eclipse.swt.widgets.Shell;
import org.eclipse.swt.widgets.Text;


public class BMEvaluateHandler implements IHandler {

	private Text textExpression;
	private Text textValue;
	
	public void addHandlerListener(IHandlerListener handlerListener) {
	}

	public void dispose() {
	}

	public Object execute(ExecutionEvent event) throws ExecutionException {
		
		String isSuspended=System.getProperty("BitMobile.isSuspended");
		if (isSuspended!=null && isSuspended.equals("false")) 
			return null;
		
		 Display display = Display.getCurrent();
		 final Shell shell = new Shell(display,SWT.APPLICATION_MODAL | SWT.SHELL_TRIM);
		 
		 Rectangle screenBounds=display.getActiveShell().getBounds();
		 	
		 
		 
		 	shell.setBounds((screenBounds.width-450)/2, (screenBounds.height-142)/2, 450,142);
			shell.setText("Evaluation");
			shell.setLayout(new GridLayout(2, false));
			
			
			
			textExpression = new Text(shell, SWT.BORDER);
			textExpression.addKeyListener(new KeyAdapter() {
				public void keyPressed(KeyEvent e) {
					if (e.keyCode==13) {
						textValue.setText(evaluate(textExpression.getText()));
					}
					if (e.keyCode==SWT.ESC) 
						shell.dispose();
				}
			});
			GridData gd_text = new GridData(SWT.LEFT, SWT.CENTER, false, false, 1, 1);
			gd_text.widthHint = 317;
			textExpression.setLayoutData(gd_text);
			
			Button btnEvaluate = new Button(shell, SWT.NONE);
			btnEvaluate.addSelectionListener(new SelectionAdapter() {
				public void widgetSelected(SelectionEvent e) {
					textValue.setText(evaluate(textExpression.getText()));
				}
			});
			btnEvaluate.addKeyListener(new KeyAdapter() {
				public void keyPressed(KeyEvent e) {
					if (e.keyCode==SWT.ESC) 
						shell.dispose();
				}
			});
			GridData gd_btnNewButton = new GridData(SWT.LEFT, SWT.CENTER, false, false, 1, 1);
			gd_btnNewButton.widthHint = 94;
			btnEvaluate.setLayoutData(gd_btnNewButton);
			btnEvaluate.setText("Evaluate");
			
			textValue = new Text(shell, SWT.BORDER);
			GridData gd_text_1 = new GridData(SWT.FILL, SWT.CENTER, true, false, 2, 1);
			gd_text_1.heightHint = 72;
			textValue.setLayoutData(gd_text_1);
			textValue.addKeyListener(new KeyAdapter() {
				public void keyPressed(KeyEvent e) {
					if (e.keyCode==SWT.ESC) 
						shell.dispose();
				}
			});

		

		 
		shell.open();
		 while (!shell.isDisposed()) {
		     if (!display.readAndDispatch()) display.sleep();
		 }
		 return null;
	}

	
	private String evaluate(String expression) {
        ILaunchManager launchManager = DebugPlugin.getDefault().getLaunchManager();
        ILaunchConfigurationType type = launchManager.getLaunchConfigurationType("Bitmobile.launchType");
        try {
            ILaunchConfiguration[] configurations = launchManager.getLaunchConfigurations(type);
            if (configurations.length>0) { 
        		String deviceAddr=configurations[0].getAttribute("DEVICE_ADDRESS", "");
        		String debugPort=configurations[0].getAttribute("DEVICE_DEBUG_PORT", "");
        		String deviceURL="http://"+deviceAddr+":"+debugPort+"/debugger/evaluate?expression="+expression;
        		try {
        			HttpURLConnection con=(HttpURLConnection)(new URL(deviceURL).openConnection());
        			con.setRequestMethod("GET");
        			BufferedReader rd=new BufferedReader(new InputStreamReader(con.getInputStream()));
        			String result=rd.readLine();
        			return result;
        		} catch (MalformedURLException e) {
        		} catch (IOException e) {
        		}
            }
        } catch (CoreException e) {
        }
        return "";
		
	}	

	
	public boolean isEnabled() {
		String isSuspended=System.getProperty("BitMobile.isSuspended");
		if (isSuspended!=null && isSuspended.equals("false")) 
			return false;
		else 
			return true;
	}

	public boolean isHandled() {
		return true;
	}

	public void removeHandlerListener(IHandlerListener handlerListener) {
	}

}
