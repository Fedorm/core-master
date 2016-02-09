abstract public class BMCommand {

    final private String fRequest;
    
    public BMCommand(String request) {
        fRequest = request;
    }
    
    public String getRequest() {
        return fRequest;
    }

    abstract public BMCommandResult createResult(String resultText);
}
