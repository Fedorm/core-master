import org.eclipse.debug.core.DebugPlugin;
import org.eclipse.debug.core.IBreakpointManager;
import org.eclipse.debug.core.model.DebugElement;
import org.eclipse.debug.core.model.IDebugTarget;


public class BMDebugElement extends DebugElement {

	public BMDebugElement(IDebugTarget target) {
		super(target);
	}
	public String getModelIdentifier() {
		return BitMobilePlugin.ID_BM_DEBUG_MODEL;
	}
    protected IBreakpointManager getBreakpointManager() {
        return DebugPlugin.getDefault().getBreakpointManager();
    }
}
