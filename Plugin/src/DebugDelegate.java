import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.UnsupportedEncodingException;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.ArrayList;
import java.util.Scanner;

import org.eclipse.core.runtime.CoreException;
import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.debug.core.DebugPlugin;
import org.eclipse.debug.core.ILaunch;
import org.eclipse.debug.core.ILaunchConfiguration;
import org.eclipse.debug.core.ILaunchManager;
import org.eclipse.debug.core.model.IDebugTarget;
import org.eclipse.debug.core.model.ILaunchConfigurationDelegate;


public class DebugDelegate implements ILaunchConfigurationDelegate {
	
	
	public void launch(ILaunchConfiguration configuration, String mode,
			ILaunch launch, IProgressMonitor monitor) throws CoreException {
		
		
			String isDebugging=System.getProperty("BitMobile.isDebugging");
			if (mode.equals(ILaunchManager.DEBUG_MODE) && isDebugging!=null && isDebugging.equals("true")) {
				BitMobilePlugin.printToConsole("сеанс отладки уже запущен");
				return;
			}
		
		

			String deployScript=configuration.getAttribute("DEPLOY_SCRIPT","");
    		String serverURL=configuration.getAttribute("SERVER_URL",IBitMobileConstants.DEFAULT_SERVER);
//    		String serverPassword=configuration.getAttribute("SERVER_PASSWORD","");
  //  		String solutionName=configuration.getAttribute("SOLUTION_NAME","");
    		String deviceAddress=configuration.getAttribute("DEVICE_ADDRESS","");
    		String deviceStarterPort=configuration.getAttribute("DEVICE_STARTER_PORT","");
    		String deviceDebugPort=configuration.getAttribute("DEVICE_DEBUG_PORT","");
			String projectPath=configuration.getAttribute("PROJECT_PATH","");
	    	
			System.out.println(projectPath);
			
  /*  		if (!new File(utilPath).isFile()) {
    			BitMobilePlugin.printToConsole("Утилита развертывания не найдена");
    			return;
    		}
			
			String[] commandList=new String[9];
			commandList[0]=utilPath;
			commandList[1]="-dr";
			commandList[2]="-host";
			commandList[3]=serverURL;
			commandList[4]="-sp";
			commandList[5]=serverPassword;
			commandList[6]="-sn";
			commandList[7]=solutionName;
			commandList[8]=projectPath;
			
			Process process;
			
			BitMobilePlugin.printToConsole(commandList[0]+" "+commandList[1]+" "+commandList[2]+" "+commandList[3]+" "+commandList[4]+" "+commandList[5]+" "+commandList[6]+" "+commandList[7]+" "+commandList[8]);
			
			try {
				process = DebugPlugin.exec(commandList, null);
			} catch (CoreException e) {
				BitMobilePlugin.printToConsole("Исключение при запуске утилиты развертывания: "+e.getMessage());
				return;
			}
			
			
			InputStream inStream=process.getInputStream();
			ArrayList<String> processOut=new ArrayList<String>();
			Scanner in;
			try {
				in=new Scanner(new InputStreamReader(inStream,"UTF-8"));
				while(in.hasNext()) {
					String nextLine=in.nextLine();
					processOut.add(nextLine);
					BitMobilePlugin.printToConsole(nextLine);
				}
				if (processOut.size()<=0){
					BitMobilePlugin.printToConsole("Пустой ответ от утилиты разветывания");
					return;
				}
				if (!processOut.get(0).equalsIgnoreCase("ok")) { 
					return;
				}
			} catch (UnsupportedEncodingException e1) {
				e1.printStackTrace();
			}

			commandList=new String[8];
			commandList[0]=utilPath;
			commandList[1]="-ar";
			commandList[2]="-host";
			commandList[3]=serverURL;
			commandList[4]="-sp";
			commandList[5]=serverPassword;
			commandList[6]="-sn";
			commandList[7]=solutionName;
			
			BitMobilePlugin.printToConsole(commandList[0]+" "+commandList[1]+" "+commandList[2]+" "+commandList[3]+" "+commandList[4]+" "+commandList[5]+" "+commandList[6]+" "+commandList[7]);
			
			try {
				process = DebugPlugin.exec(commandList, null);
			} catch (CoreException e) {
				BitMobilePlugin.printToConsole("Исключение при запуске утилиты развертывания: "+e.getMessage());
				return;
			}

			inStream=process.getInputStream();
			try {
				in=new Scanner(new InputStreamReader(inStream,"UTF-8"));
				processOut=new ArrayList<String>();
				while(in.hasNext()) {
					String nextLine=in.nextLine();
					processOut.add(nextLine);
					BitMobilePlugin.printToConsole(nextLine);
				}
				if (processOut.size()<=0){
					BitMobilePlugin.printToConsole("Пустой ответ от утилиты разветывания");
					return;
				}
				if (!processOut.get(0).equalsIgnoreCase("ok")) { 
					return;
				}
			} catch (UnsupportedEncodingException e1) {
				e1.printStackTrace();
			}*/
			
			deployScript=deployScript.replace("%HOST%", serverURL);
			deployScript=deployScript.replace("%PATH%", projectPath);
			
			String[] deployArray=deployScript.split("\r\n");
			
			for(String cmd : deployArray) {
				Process process;
				
				BitMobilePlugin.printToConsole(cmd);
				String[] cmdArray=cmd.split(" ");
				
				try {
					process = DebugPlugin.exec(cmdArray, null);
				} catch (CoreException e) {
					BitMobilePlugin.printToConsole("Исключение при запуске утилиты развертывания: "+e.getMessage());
					return;
				}

				InputStream inStream=process.getInputStream();
				ArrayList<String> processOut=new ArrayList<String>();
				Scanner in;
				try {
					in=new Scanner(new InputStreamReader(inStream,"cp866"));
					while(in.hasNext()) {
						String nextLine=in.nextLine();
						processOut.add(nextLine);
						BitMobilePlugin.printToConsole(nextLine);
					}
					if (processOut.size()<=0){
						BitMobilePlugin.printToConsole("Пустой ответ от утилиты разветывания");
						return;
					}
					if (!processOut.get(0).equalsIgnoreCase("ok")) { 
						return;
					}
				} catch (UnsupportedEncodingException e1) {
					e1.printStackTrace();
				}
				
			}
			
			
			String deviceURL="http://"+deviceAddress+":"+deviceStarterPort+"/debugger/run";
		//	BitMobilePlugin.printToConsole("Пытаемся вызвать "+deviceURL);
			
			try {
				HttpURLConnection con=(HttpURLConnection)(new URL(deviceURL).openConnection());
				con.setRequestMethod("GET");
			//	BitMobilePlugin.printToConsole(new Integer(con.getResponseCode()).toString());
				if (con.getResponseCode()==200) {
					if (mode.equals(ILaunchManager.DEBUG_MODE)) {
						IDebugTarget target = new BMDebugTarget(launch,deviceAddress,Integer.parseInt(deviceDebugPort));
						System.setProperty("BitMobile.isDebugging", "true");
						launch.addDebugTarget(target);
						return;
					}
				}
				
			} catch (MalformedURLException e) {
				BitMobilePlugin.printToConsole("Ошибка в URL: " + e.getMessage());
			} catch (IOException e) {
				BitMobilePlugin.printToConsole("Ошибка соединения: " + e.getMessage());
			} catch (NumberFormatException e) {
				BitMobilePlugin.printToConsole("Некорректное значение порта отладки");
			} catch (IllegalArgumentException e) {
				BitMobilePlugin.printToConsole("Некорректное значение порта управления");
			}
			
			
    			
		}		
	

}
