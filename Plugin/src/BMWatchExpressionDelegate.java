
import org.eclipse.debug.core.DebugException;
import org.eclipse.debug.core.model.IDebugElement;
import org.eclipse.debug.core.model.IValue;
import org.eclipse.debug.core.model.IWatchExpressionDelegate;
import org.eclipse.debug.core.model.IWatchExpressionListener;
import org.eclipse.debug.core.model.IWatchExpressionResult;


public class BMWatchExpressionDelegate implements IWatchExpressionDelegate {

	class WatchResult implements IWatchExpressionResult {

		IValue value=null;
		String expression;
		
		public WatchResult(String expr) {
			expression=expr;
		}
		
		@Override
		public IValue getValue() {
			// TODO Auto-generated method stub
			return value;
		}

		@Override
		public boolean hasErrors() {
			// TODO Auto-generated method stub
			return false;
		}

		@Override
		public String[] getErrorMessages() {
			// TODO Auto-generated method stub
			return null;
		}

		@Override
		public String getExpressionText() {
			// TODO Auto-generated method stub
			return expression;
		}

		@Override
		public DebugException getException() {
			// TODO Auto-generated method stub
			return null;
		}
		
	}
	
	
	@Override
	public void evaluateExpression(String expression, IDebugElement context,
			IWatchExpressionListener listener) {
		WatchResult result=new WatchResult(expression);
		BMDebugTarget target=null;
		if (context instanceof BMDebugTarget) {
			target=(BMDebugTarget)context;
		}
		else if (context instanceof BMStackFrame) {
			target=(BMDebugTarget)((BMStackFrame)context).getDebugTarget();
		}
		
		if (target!=null) {
			try {
				System.out.println("evaluating "+expression);
/*				if (!expression.matches("[A-Za-z_]")) 
					result.value=new BMValue(target.sendCommand(new BMEvaluateCommand(expression)).fResponseText);
				else*/	
					result.value=new BMVariable(target,expression).getValue();
			} catch (DebugException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			} 
		}
		else System.out.println(context.toString());
		
		listener.watchEvaluationFinished(result);

	}

}
