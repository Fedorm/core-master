import org.eclipse.core.runtime.IAdapterFactory;
import org.eclipse.debug.internal.ui.viewers.model.provisional.IElementContentProvider;
import org.eclipse.debug.internal.ui.viewers.model.provisional.IModelProxyFactory;
import org.eclipse.debug.internal.ui.viewers.model.provisional.IViewActionProvider;


/**
 * 
 * @since 3.2
 *
 */
@SuppressWarnings("rawtypes")
public class SFAdapterFactory implements IAdapterFactory {
	
	private static IElementContentProvider fgTargetAdapter = new BMDebugTargetContentProvider();
	private static IModelProxyFactory fgFactory = new ModelProxyFactory();
	private static IViewActionProvider fgViewActionProvider = new BMViewActionProvider();

	public Object getAdapter(Object adaptableObject, Class adapterType) {
		if (IElementContentProvider.class.equals(adapterType)) {
			if (adaptableObject instanceof BMDebugTarget) {
				return fgTargetAdapter;
			}
		}
		if (IModelProxyFactory.class.equals(adapterType)) {
			if (adaptableObject instanceof BMDebugTarget) {
				return fgFactory;
			}
		}
		if (IViewActionProvider.class.equals(adapterType)) {
			if (adaptableObject instanceof BMStackFrame) {
				return fgViewActionProvider;
			}
		}
		return null;
	}

	public Class[] getAdapterList() {
		return new Class[]{IElementContentProvider.class, IModelProxyFactory.class, IViewActionProvider.class};
	}

}
