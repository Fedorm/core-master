import org.eclipse.core.resources.IProject;
import org.eclipse.core.resources.IResource;
import org.eclipse.core.resources.ProjectScope;
import org.eclipse.core.runtime.CoreException;
import org.eclipse.core.runtime.preferences.IEclipsePreferences;
import org.eclipse.core.runtime.preferences.IScopeContext;
import org.eclipse.debug.core.DebugPlugin;
import org.eclipse.debug.core.ILaunchConfiguration;
import org.eclipse.debug.core.ILaunchConfigurationType;
import org.eclipse.debug.core.ILaunchConfigurationWorkingCopy;
import org.eclipse.debug.core.ILaunchManager;
import org.eclipse.debug.ui.DebugUITools;
import org.eclipse.debug.ui.ILaunchShortcut;
import org.eclipse.jface.viewers.ISelection;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.PlatformUI;


public class DebugShortcut implements ILaunchShortcut {

	private void doLaunch(IResource resource,String mode) {
		
		if (resource==null) {
			System.out.println("Нет активного ресурса");
			return;
		}
		
		IProject project=resource.getProject();
		if (project==null) {
			System.out.println("Нет текущего проекта");
			return;
		}
		
        String qualifier = BitMobilePlugin.getDefault().getBundle().getSymbolicName();
    	IScopeContext context = new ProjectScope(project);
    	IEclipsePreferences node = context.getNode(qualifier);
    	if (node == null) {
    		System.out.println("Не заданы настройки проекта");
			return;
    	}
    		
		String deployScript=node.get(IBitMobileConstants.DEPLOY_SCRIPT,"");
		String serverURL=node.get(IBitMobileConstants.SERVER_URL,IBitMobileConstants.DEFAULT_SERVER);
//		String serverPassword=node.get(IBitMobileConstants.SERVER_PASSWORD,"");
//		String solutionName=node.get(IBitMobileConstants.SOLUTION_NAME,"");
		String deviceAddress=node.get(IBitMobileConstants.DEVICE_ADDRESS,"");
		String deviceStarterPort=node.get(IBitMobileConstants.DEVICE_STARTER_PORT,"");
		String deviceDebugPort=node.get(IBitMobileConstants.DEVICE_DEBUG_PORT,"");
		
		
        ILaunchManager launchManager = DebugPlugin.getDefault().getLaunchManager();
        ILaunchConfigurationType type = launchManager.getLaunchConfigurationType("Bitmobile.launchType");
        try {
        	
       	
            ILaunchConfiguration[] configurations = launchManager.getLaunchConfigurations(type);
            
            ILaunchConfigurationWorkingCopy configurationCopy;

            if (configurations.length>0) 
            		configurationCopy=configurations[0].getWorkingCopy();
            
            else 
                configurationCopy = type.newInstance(null, "Bitmobile launch");

            
            
            configurationCopy.setAttribute("DEPLOY_SCRIPT", deployScript);
    		configurationCopy.setAttribute("SERVER_URL", serverURL);
//    		configurationCopy.setAttribute("SERVER_PASSWORD", serverPassword);
  //  		configurationCopy.setAttribute("SOLUTION_NAME", solutionName);
    		configurationCopy.setAttribute("DEVICE_ADDRESS", deviceAddress);
    		configurationCopy.setAttribute("DEVICE_STARTER_PORT", deviceStarterPort);
    		configurationCopy.setAttribute("DEVICE_DEBUG_PORT", deviceDebugPort);
    		configurationCopy.setAttribute("PROJECT_PATH", project.getLocation().toOSString());
    		configurationCopy.setMappedResources(new IResource[]{resource});    		
    		ILaunchConfiguration configuration=configurationCopy.doSave();
    		
    		

    		
    		
            DebugUITools.launch(configuration, mode);
            return;
            	
            
        } catch (CoreException e) {
            return;
        }
        
		
	}
	
	@Override
	public void launch(ISelection selection, String mode) {
		System.out.println(PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().getActiveEditor().getClass().toString());
		doLaunch(BitMobilePlugin.extractSelection(selection),mode);

	}

	@Override
	public void launch(IEditorPart editor, String mode) {
		doLaunch(BitMobilePlugin.extractResource(editor),mode);

	}

}
