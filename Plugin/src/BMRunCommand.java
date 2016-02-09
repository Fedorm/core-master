public class BMRunCommand extends BMCommand {

    public BMRunCommand() {
        super("run");
    }
    

    public BMCommandResult createResult(String resultText) {
        return new BMCommandResult(resultText);
    }
}
