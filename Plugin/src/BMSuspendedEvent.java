
public class BMSuspendedEvent extends BMEvent {
    
    public BMSuspendedEvent(String command,String args) {
        super(command,args);
    }
    
    public static boolean isEventMessage(String message) {
        return message.startsWith("suspended");
    }
    public static boolean isEventMessage(String command,String args) {
        return command.equals("/debugger/suspended");
    }
}
