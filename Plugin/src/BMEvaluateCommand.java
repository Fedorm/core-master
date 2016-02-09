public class BMEvaluateCommand extends BMCommand {

    public BMEvaluateCommand(String expression) {
        super("evaluate?expression="+expression);
    }
    

    public BMCommandResult createResult(String resultText) {
        return new BMCommandResult(resultText);
    }
}
