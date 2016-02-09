import org.eclipse.debug.core.sourcelookup.AbstractSourceLookupDirector;
import org.eclipse.debug.core.sourcelookup.ISourceLookupParticipant;


public class BMSourceLocator extends AbstractSourceLookupDirector {

	public void initializeParticipants() {
		System.out.println("init participants");
		addParticipants(new ISourceLookupParticipant[]{new BMSourceLookupParticipant()});

	}

}
