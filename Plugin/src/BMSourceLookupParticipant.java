import org.eclipse.core.runtime.CoreException;
import org.eclipse.debug.core.sourcelookup.AbstractSourceLookupParticipant;


public class BMSourceLookupParticipant extends AbstractSourceLookupParticipant {

	@Override
	public String getSourceName(Object object) throws CoreException {
		if (object instanceof BMStackFrame) {
			System.out.println("getsourcename stackframe"+((BMStackFrame)object).getName());
			return ((BMStackFrame)object).getName();
			
		}
		System.out.println("getsourcename "+object.toString());
		return null;
	}

}
