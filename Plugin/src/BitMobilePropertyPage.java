import org.eclipse.core.resources.ProjectScope;
import org.eclipse.core.runtime.CoreException;
import org.eclipse.core.runtime.preferences.IEclipsePreferences;
import org.eclipse.core.runtime.preferences.IScopeContext;
import org.eclipse.debug.core.DebugPlugin;
import org.eclipse.debug.core.ILaunchConfiguration;
import org.eclipse.debug.core.ILaunchConfigurationType;
import org.eclipse.debug.core.ILaunchManager;
import org.eclipse.swt.SWT;
import org.eclipse.swt.events.ModifyEvent;
import org.eclipse.swt.events.ModifyListener;
import org.eclipse.swt.events.SelectionEvent;
import org.eclipse.swt.events.SelectionListener;
import org.eclipse.swt.layout.GridData;
import org.eclipse.swt.layout.GridLayout;
import org.eclipse.swt.widgets.Button;
import org.eclipse.swt.widgets.Composite;
import org.eclipse.swt.widgets.Control;
import org.eclipse.swt.widgets.FileDialog;
import org.eclipse.swt.widgets.Group;
import org.eclipse.swt.widgets.Label;
import org.eclipse.swt.widgets.Text;
import org.eclipse.ui.IWorkbench;
import org.eclipse.ui.dialogs.PropertyPage;
import org.osgi.service.prefs.BackingStoreException;


public class BitMobilePropertyPage extends PropertyPage implements
        SelectionListener, ModifyListener {
    
    private Button btnBrowse;

    private Text edtUtilPath;
    private Text edtServerURL;
//    private Text edtServerPassword;
    //private Text edtSolutionName;
    private Text edtDeviceAddress;
    private Text edtDeviceStarterPort;
    private Text edtDeviceDebugPort;
    private Text edtDeployScript;

    


    private Group createGroup(Composite parent, int numColumns) {
        Group group = new Group(parent, SWT.NULL);

        //GridLayout
        GridLayout layout = new GridLayout();
        layout.numColumns = numColumns;
        group.setLayout(layout);

        //GridData
        GridData data = new GridData();
        data.verticalAlignment = GridData.FILL;
        data.horizontalAlignment = GridData.FILL;
        group.setLayoutData(data);
        return group;
    }


    protected Control createContents(Composite parent) {
//    	PlatformUI.getWorkbench().getHelpSystem().setHelp(parent,
//				IBitmobileConstants.PREFERENCE_PAGE_CONTEXT);

    	
        
        Group composite_deploy=createGroup(parent,1);
        composite_deploy.setText("Deploy parameters");

        
        
//        GridData data = new GridData();
//        data.horizontalSpan = 2;
        //data.horizontalAlignment = GridData.FILL;
        //grp.setLayoutData(data);
//        Label label = new Label(grp, SWT.NONE);
        //label.setText("Two buttons:");
        //label.pack();
        
        
   //     createLabel(composite_deploy, "Deploy util path"); //$NON-NLS-1$
    //   edtUtilPath = createTextField(composite_deploy);
     //   btnBrowse=createPushButton(composite_deploy, "Browse"); //$NON-NLS-1$

        createLabel(composite_deploy, "Server Host"); //$NON-NLS-1$
        edtServerURL = createTextField(composite_deploy);
        
//        createLabel(composite_deploy, "Solution password"); //$NON-NLS-1$
 //       edtServerPassword = createTextField(composite_deploy);
        createLabel(composite_deploy, "Deploy script");
        edtDeployScript = createMemo(composite_deploy);
        
//        createLabel(composite_deploy, "Solution name"); //$NON-NLS-1$
 //       edtSolutionName = createTextField(composite_deploy);
        
        Group composite_debug=createGroup(parent,1);
        composite_deploy.setText("Debug parameters");
                
        createLabel(composite_debug, "Device address"); //$NON-NLS-1$
        edtDeviceAddress = createTextField(composite_debug);
        
        createLabel(composite_debug, "Device starter port"); //$NON-NLS-1$
        edtDeviceStarterPort = createTextField(composite_debug);

        createLabel(composite_debug, "Device debugging port"); //$NON-NLS-1$
        edtDeviceDebugPort = createTextField(composite_debug);
        
        initializeValues();

        //font = null;
        return new Composite(parent, SWT.NULL);
    }


    private Label createLabel(Composite parent, String text) {
        Label label = new Label(parent, SWT.LEFT);
        label.setText(text);
        GridData data = new GridData();
        data.horizontalSpan = 2;
        data.horizontalAlignment = GridData.FILL;
        label.setLayoutData(data);
        return label;
    }






    private Text createTextField(Composite parent) {
        Text text = new Text(parent, SWT.SINGLE | SWT.BORDER);
        text.addModifyListener(this);
        GridData data = new GridData();
        data.horizontalAlignment = GridData.FILL;
        data.grabExcessHorizontalSpace = true;
        data.verticalAlignment = GridData.CENTER;
        data.grabExcessVerticalSpace = false;
        text.setLayoutData(data);
        return text;
    }
    
    private Text createMemo(Composite parent) {
        Text text = new Text(parent, SWT.MULTI | SWT.BORDER);
        text.addModifyListener(this);
        GridData data = new GridData();
        data.heightHint = 48;
        data.horizontalAlignment = GridData.FILL;
        data.grabExcessHorizontalSpace = true;
        data.verticalAlignment = GridData.FILL;
        data.grabExcessVerticalSpace = true;
        data.horizontalSpan=2;
        text.setLayoutData(data);
        return text;
    }


    public void init(IWorkbench workbench) {
 
    }


    private void initializeDefaults() {
  //      edtUtilPath.setText("");
        edtServerURL.setText(IBitMobileConstants.DEFAULT_SERVER);
 //       edtServerPassword.setText("");
  //      edtSolutionName.setText("");
        edtDeviceAddress.setText("");
        edtDeviceStarterPort.setText("");
        edtDeployScript.setText("");
    }


    private void initializeValues() {
//        String qualifier = BitMobilePlugin.getDefault().getDescriptor().getUniqueIdentifier();
    	String qualifier = BitMobilePlugin.getDefault().getBundle().getSymbolicName(); 
    	IScopeContext context = new ProjectScope(BitMobilePlugin.getActiveResource().getProject());
    	IEclipsePreferences node = context.getNode(qualifier);
    	if (node != null) {
 //   		edtUtilPath.setText(node.get(IBitMobileConstants.UTIL_PATH,""));
    		edtServerURL.setText(node.get(IBitMobileConstants.SERVER_URL,IBitMobileConstants.DEFAULT_SERVER));
    		edtDeployScript.setText(node.get(IBitMobileConstants.DEPLOY_SCRIPT,""));
  //  		edtServerPassword.setText(node.get(IBitMobileConstants.SERVER_PASSWORD,""));
    //		edtSolutionName.setText(node.get(IBitMobileConstants.SOLUTION_NAME,""));
    		edtDeviceAddress.setText(node.get(IBitMobileConstants.DEVICE_ADDRESS,""));
    		edtDeviceStarterPort.setText(node.get(IBitMobileConstants.DEVICE_STARTER_PORT,""));
    		edtDeviceDebugPort.setText(node.get(IBitMobileConstants.DEVICE_DEBUG_PORT,""));
    	}
    }

    public void modifyText(ModifyEvent event) {
    }

    protected void performDefaults() {
        super.performDefaults();
        initializeDefaults();
    }




    public void widgetDefaultSelected(SelectionEvent event) {
    }

    public void widgetSelected(SelectionEvent event) {

    	if (event.widget==btnBrowse) {
    		   FileDialog dialog = new FileDialog(getShell(), SWT.OPEN);
    		   dialog.setFilterExtensions(new String [] {"*.exe"});
    		   String result = dialog.open();
    		   if (result!=null) {
    			   edtUtilPath.setText(result);
    		   }
    	}
    }

	
	  public boolean performOk() {
//	        String qualifier = BitMobilePlugin.getDefault().getDescriptor().getUniqueIdentifier();
		  	String qualifier = BitMobilePlugin.getDefault().getBundle().getSymbolicName();
	    	IScopeContext context = new ProjectScope(BitMobilePlugin.getActiveResource().getProject());
	    	IEclipsePreferences node = context.getNode(qualifier);
	    	if (node != null) {
//	    	    node.put(IBitMobileConstants.UTIL_PATH,edtUtilPath.getText());
	    	    node.put(IBitMobileConstants.SERVER_URL,edtServerURL.getText());
	    	    node.put(IBitMobileConstants.DEPLOY_SCRIPT,edtDeployScript.getText());

	    	    //	    	    node.put(IBitMobileConstants.SERVER_PASSWORD,edtServerPassword.getText());
//	    	    node.put(IBitMobileConstants.SOLUTION_NAME,edtSolutionName.getText());
	    	    node.put(IBitMobileConstants.DEVICE_ADDRESS,edtDeviceAddress.getText());
	    	    node.put(IBitMobileConstants.DEVICE_STARTER_PORT,edtDeviceStarterPort.getText());
	    	    node.put(IBitMobileConstants.DEVICE_DEBUG_PORT,edtDeviceDebugPort.getText());
	    	    try {
					node.flush();
				} catch (BackingStoreException e) {
					// TODO Auto-generated catch block
					
				}
	            ILaunchManager launchManager = DebugPlugin.getDefault().getLaunchManager();
	            ILaunchConfigurationType type = launchManager.getLaunchConfigurationType("Bitmobile.launchType");
                ILaunchConfiguration[] configurations;
				try {
					configurations = launchManager.getLaunchConfigurations(type);
	                for(int i = 0;i<configurations.length;i++) 
	                	configurations[i].delete();	    	    
				} catch (CoreException e) {
					
				}
	    	    
	    	}
	        return true;
	    }
		
	    
			
}
