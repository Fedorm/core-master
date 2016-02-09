public class BMTerminateCommand extends BMCommand {

    public BMTerminateCommand() {
        super("terminate");
    }
    

    public BMCommandResult createResult(String resultText) {
        return new BMCommandResult(resultText);
    }
}
