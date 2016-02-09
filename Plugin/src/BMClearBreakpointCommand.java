

public class BMClearBreakpointCommand extends BMCommand {

    public BMClearBreakpointCommand(String filePath,int lineNumber) {
        super("clearbreakpoint?breakpoint="+filePath+':'+lineNumber);
    }
    

    public BMCommandResult createResult(String resultText) {
        return new BMCommandResult(resultText);
    }
}
