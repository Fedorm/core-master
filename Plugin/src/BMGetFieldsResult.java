
import java.util.ArrayList;
import java.util.StringTokenizer;



public class BMGetFieldsResult extends BMCommandResult {
    
    final public String[] fValues;
    
    BMGetFieldsResult(String response) {
        super(response);
        if(response==null || response.isEmpty()) {
        	fValues=new String[0];
        	return;
        }
        StringTokenizer st = new StringTokenizer(response, "\r\n");
        ArrayList<String> valuesList = new ArrayList<String>();
        
        while (st.hasMoreTokens()) {
            String token = st.nextToken();
            if (token.length() != 0) {
                valuesList.add(token);
            }
        }

        fValues = valuesList.toArray(new String[valuesList.size()]);
    }
}
