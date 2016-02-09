public class BMGetFieldsCommand extends BMCommand {

    public BMGetFieldsCommand(String expression) {
        super("getfields?expression="+expression);
    }
    

    public BMCommandResult createResult(String resultText) {
        return new BMGetFieldsResult(resultText);
    }
}
