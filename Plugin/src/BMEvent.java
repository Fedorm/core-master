public class BMEvent {
    public final String fCommand;
    public final String fArgs;
    

    public BMEvent(String command, String args) {
        fCommand = command;
        fArgs = args;
    }

    protected String getName(String message) {
        return fCommand;
    }
    

    
    public static BMEvent parseEvent(String command,String args) {
        if (BMTerminatedEvent.isEventMessage(command,args)) {
            return new BMTerminatedEvent(command,args);
        } 
        else if (BMStartedEvent.isEventMessage(command,args)) {
            return new BMStartedEvent(command,args);
        } 
        else if (BMSuspendedEvent.isEventMessage(command,args)) {
            return new BMSuspendedEvent(command,args);
        } 
        else {
            return new BMEvent(command,args);
        }    	
    }
}
