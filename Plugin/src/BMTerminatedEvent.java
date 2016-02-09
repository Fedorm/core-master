
public class BMTerminatedEvent extends BMEvent {
    
    public BMTerminatedEvent(String command,String args) {
        super(command,args);
    }
    
    public static boolean isEventMessage(String message) {
        return message.startsWith("terminated");
    }
    public static boolean isEventMessage(String command,String args) {
        return command.equals("/debugger/terminated");
    }
}
