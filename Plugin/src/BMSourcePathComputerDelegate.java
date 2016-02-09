import org.eclipse.core.resources.IProject;
import org.eclipse.core.resources.IResource;
import org.eclipse.core.resources.ResourcesPlugin;
import org.eclipse.core.runtime.CoreException;
import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.core.runtime.Path;
import org.eclipse.debug.core.ILaunchConfiguration;
import org.eclipse.debug.core.sourcelookup.ISourceContainer;
import org.eclipse.debug.core.sourcelookup.ISourcePathComputerDelegate;
import org.eclipse.debug.core.sourcelookup.containers.ProjectSourceContainer;
import org.eclipse.debug.core.sourcelookup.containers.WorkspaceSourceContainer;


public class BMSourcePathComputerDelegate implements
		ISourcePathComputerDelegate {

	public ISourceContainer[] computeSourceContainers(
			ILaunchConfiguration configuration, IProgressMonitor monitor)
			throws CoreException {
		System.out.println("computesourcecontainers");
		ISourceContainer sourceContainer = null;
		Path projectPath=new Path(configuration.getAttribute("PROJECT_PATH",""));
//		String projectName=projectPath.lastSegment();
		String projectFolder=projectPath.lastSegment();
		IResource resource=null;
		
		IProject[] projects=ResourcesPlugin.getWorkspace().getRoot().getProjects();
		for(IProject prj : projects) {
			if (prj.getFullPath().lastSegment().equals(projectFolder)) {
				System.out.println("projectName "+prj.getName());
				resource=prj;
			}
		}
		
		
//		IResource resource = ResourcesPlugin.getWorkspace().getRoot().getProject(projectName);
		if (resource != null) {
			sourceContainer = new ProjectSourceContainer((IProject) resource, false);
		}
		else {
			sourceContainer = new WorkspaceSourceContainer();
		}
		
		System.out.println(sourceContainer.toString());
		System.out.println(sourceContainer.findSourceElements("/device/app/Script/Main/Main.js"));
		
		return new ISourceContainer[]{sourceContainer};	}

}
