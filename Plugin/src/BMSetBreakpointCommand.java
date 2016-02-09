public class BMSetBreakpointCommand extends BMCommand {

    public BMSetBreakpointCommand(String filePath,int lineNumber) {
        super("setbreakpoint?breakpoint="+filePath+':'+lineNumber);
    }
    

    public BMCommandResult createResult(String resultText) {
        return new BMCommandResult(resultText);
    }
}
