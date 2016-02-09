
public class BMResumeCommand extends BMCommand {

    public BMResumeCommand() {
        super("resume");
    }
    

    public BMCommandResult createResult(String resultText) {
        return new BMCommandResult(resultText);
    }

}
