
public class BMStartedEvent extends BMEvent {
    
    public BMStartedEvent(String command,String args) {
        super(command,args);
    }
    
    public static boolean isEventMessage(String message) {
        return message.startsWith("started");
    }
    public static boolean isEventMessage(String command,String args) {
        return command.equals("/debugger/started");
    }
}
