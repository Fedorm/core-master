import java.awt.Toolkit;
import java.awt.datatransfer.Clipboard;
import java.awt.datatransfer.StringSelection;
import java.io.BufferedReader;
import java.io.IOException;
import java.net.HttpURLConnection;
import java.net.URL;
import java.net.URLEncoder;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;

import org.eclipse.core.resources.ProjectScope;
import org.eclipse.core.runtime.preferences.IEclipsePreferences;
import org.eclipse.core.runtime.preferences.IScopeContext;
import org.eclipse.swt.SWT;
import org.eclipse.swt.custom.TableCursor;
import org.eclipse.swt.events.KeyAdapter;
import org.eclipse.swt.events.KeyEvent;
import org.eclipse.swt.events.SelectionAdapter;
import org.eclipse.swt.events.SelectionEvent;
import org.eclipse.swt.layout.GridData;
import org.eclipse.swt.layout.GridLayout;
import org.eclipse.swt.widgets.Button;
import org.eclipse.swt.widgets.Composite;
import org.eclipse.swt.widgets.Menu;
import org.eclipse.swt.widgets.MenuItem;
import org.eclipse.swt.widgets.Table;
import org.eclipse.swt.widgets.TableColumn;
import org.eclipse.swt.widgets.TableItem;
import org.eclipse.swt.widgets.Text;
import org.eclipse.ui.part.ViewPart;
import org.w3c.dom.Document;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;



public class BMDataView extends ViewPart {

	private Text text;
	private Table table;
	private TableCursor tableCursor;
	public BMDataView() {
		
	}
	
	
	
	public void createPartControl(Composite parent) {
		parent.setLayout(new GridLayout(2, false));
		
		text = new Text(parent, SWT.BORDER);
		GridData gd_text = new GridData(SWT.FILL, SWT.CENTER, true, false, 1, 1);
		gd_text.widthHint = 211;
		text.setLayoutData(gd_text);
		
		text.addKeyListener(new KeyAdapter() {
			public void keyPressed(KeyEvent e) {
				if (e.keyCode==13) 
					query();
			}
		});
		
		
		Button btnNewButton = new Button(parent, SWT.NONE);
		btnNewButton.setText("Query");
		btnNewButton.addSelectionListener(new SelectionAdapter() {
			public void widgetSelected(SelectionEvent e) {
				query();
			}
		});
		
		
		table = new Table(parent, SWT.BORDER | SWT.FULL_SELECTION);
		table.setLayoutData(new GridData(SWT.FILL, SWT.FILL, true, true, 2, 1));
		table.setHeaderVisible(true);
		table.setLinesVisible(true);
		
		Menu menu = new Menu(table);
		table.setMenu(menu);
		
		MenuItem mntmCopy = new MenuItem(menu, SWT.POP_UP);
		mntmCopy.addSelectionListener(new SelectionAdapter() {
			public void widgetSelected(SelectionEvent e) {
				if (tableCursor.getRow()==null) 
					return;
				Toolkit toolkit = Toolkit.getDefaultToolkit();
				Clipboard clipboard = toolkit.getSystemClipboard();
				StringSelection strSel = new StringSelection(tableCursor.getRow().getText(tableCursor.getColumn()));				
				clipboard.setContents(strSel, null);
			}
		});
		mntmCopy.setText("Copy");
		
		tableCursor = new TableCursor(table, SWT.NONE);
		tableCursor.setMenu(menu);
		
		
	/*	TableColumn tblclmnCol = new TableColumn(table, SWT.NONE);
		tblclmnCol.setWidth(100);
		tblclmnCol.setText("col1");
		
		TableColumn tblclmnCol_1 = new TableColumn(table, SWT.NONE);
		tblclmnCol_1.setWidth(100);
		tblclmnCol_1.setText("col2");
		
		TableColumn tblclmnCol_2 = new TableColumn(table, SWT.NONE);
		tblclmnCol_2.setWidth(100);
		tblclmnCol_2.setText("col3");
		
		TableItem tableItem = new TableItem(table, SWT.NONE);
		tableItem.setText(new String[] {"1", "2", "3"});
		
		TableItem tableItem_1 = new TableItem(table, SWT.NONE);
		tableItem_1.setText(new String[] {"\u043F\u0440\u0435\u0432\u0435\u0434", "\u043C\u0435\u0434\u0432\u0435\u0434", "\u0438\u043D\u0442\u0435\u0440\u043D\u0435\u0434"});
//		new Label(this, SWT.NONE);
		//new Label(this, SWT.NONE);*/

	
		//parent.setLayout(new GridLayout(1, false));
		//parent.setLayout(new FillLayout(SWT.NONE));
		
/*		Button btnConnect = new Button(parent, SWT.NONE);
		btnConnect.addSelectionListener(new SelectionAdapter() {
			public void widgetSelected(SelectionEvent e) {

				IProject project=BitMobilePlugin.getActiveResource().getProject();
				if (project==null) {
					System.out.println("Нет текущего проекта");
					return;
				}
				
		        String qualifier = BitMobilePlugin.getDefault().getBundle().getSymbolicName();
		    	IScopeContext context = new ProjectScope(project);
		    	IEclipsePreferences node = context.getNode(qualifier);
		    	if (node == null) {
		    		System.out.println("Не заданы настройки проекта");
					return;
		    	}
		    		
				String serverAddr=node.get(IBitMobileConstants.DEVICE_ADDRESS,"");
//				browser.setUrl("http://"+serverAddr+":8083/query");
				browser.setUrl("http://yandex.ru");
				
				
			}
		});
		

		btnConnect.setText("Connect");
		*/
		
//		browser = new Browser(parent, SWT.NONE);
		
//		browser.setLayoutData(new GridData(SWT.LEFT, SWT.CENTER, true, true, 1, 1));

	}
	
	public String readBigStringIn(BufferedReader buffIn) {
        StringBuilder b = new StringBuilder();

        try {
            String line = buffIn.readLine();
            while (line != null) {
                b.append(line);
                line = buffIn.readLine();
            }
        }
        catch(IOException e){};

        return b.toString();
}	

	private void query() {
		
		table.removeAll();
		
		
		if (table.getColumnCount()>0) {
			for(TableColumn column : table.getColumns()) 
				column.dispose();
		}
		
		
		
		//String queryResult="";
		Document doc=null;
		
    	String qualifier = BitMobilePlugin.getDefault().getBundle().getSymbolicName(); 
    	IScopeContext context = new ProjectScope(BitMobilePlugin.getActiveResource().getProject());
    	IEclipsePreferences node = context.getNode(qualifier);
    	if (node != null) {
			try {
				String deviceURL="http://"+node.get(IBitMobileConstants.DEVICE_ADDRESS,"")+":8083"+"/xmlresult?query="+URLEncoder.encode(text.getText(),"cp1251");
				System.out.println(node.get(IBitMobileConstants.DEVICE_ADDRESS,""));
				System.out.println(node.get(IBitMobileConstants.SOLUTION_NAME,""));
				System.out.println(deviceURL);
				
				if (node.get(IBitMobileConstants.DEVICE_ADDRESS,"").isEmpty()) { 
					System.out.println("empty address");
					return;
				}	
				HttpURLConnection con=(HttpURLConnection)(new URL(deviceURL).openConnection());
				con.setRequestMethod("GET");
//				BufferedReader rd=new BufferedReader(new InputStreamReader(con.getInputStream()));
				//queryResult=readBigStringIn(rd);
				DocumentBuilderFactory dbFactory = DocumentBuilderFactory.newInstance();
				DocumentBuilder dBuilder = dbFactory.newDocumentBuilder();
				doc = dBuilder.parse(con.getInputStream());
				System.out.println(doc.getChildNodes().getLength());
				
				
			} catch (Exception e) {
				e.printStackTrace();
			}
    	}
    	else 
    		System.out.println("project not found");
			
		
		if (doc==null) 
			return;
		
		
		
		NodeList docNodeList=doc.getChildNodes().item(0).getChildNodes();
/*		ArrayList<Integer> sizes=new ArrayList<Integer>();
		boolean firstPass=true;
		
		for (int i=0;i<docNodeList.getLength();i++) {
			Node resultNode=docNodeList.item(i);
			int fieldNo;
			fieldNo=0;
			if (resultNode.getNodeType()==Node.ELEMENT_NODE) {
				NodeList resultSubNodeList=resultNode.getChildNodes();
				for(int j=0;j<resultSubNodeList.getLength();j++) {
					Node resultSubNode=resultSubNodeList.item(j);
					if (resultSubNode.getNodeType()==Node.ELEMENT_NODE) {
						if (firstPass) 
							sizes.add(0);
						String value=resultSubNode.getTextContent();
						if (value.length()>sizes.get(fieldNo)) {
							sizes.set(fieldNo, value.length());
						}
						fieldNo++;
					}
				}
				firstPass=false;
					
				
			}
		} */
		


		for (int i=0;i<docNodeList.getLength();i++) {
			Node resultNode=docNodeList.item(i);
			if (resultNode.getNodeType()==Node.ELEMENT_NODE) {
				NodeList resultSubNodeList=resultNode.getChildNodes();
				for(int j=0;j<resultSubNodeList.getLength();j++) {
					Node resultSubNode=resultSubNodeList.item(j);
					if (resultSubNode.getNodeType()==Node.ELEMENT_NODE) {
						TableColumn column=new TableColumn(table,SWT.NONE);
						column.setText(resultSubNode.getNodeName());
					}
				}
				break;	
			}
		} 
		
		
		for (int i=0;i<docNodeList.getLength();i++) {
			Node resultNode=docNodeList.item(i);
			int fieldNo;
			fieldNo=0;
			if (resultNode.getNodeType()==Node.ELEMENT_NODE) {
				NodeList resultSubNodeList=resultNode.getChildNodes();
				TableItem item=new TableItem(table,SWT.NONE);
				String[] items=new String[table.getColumnCount()];
				for(int j=0;j<resultSubNodeList.getLength();j++) {
					Node resultSubNode=resultSubNodeList.item(j);
					if (resultSubNode.getNodeType()==Node.ELEMENT_NODE) {
						items[fieldNo]=resultSubNode.getTextContent();
						fieldNo++;
					}
				}
				item.setText(items);
			}
		} 
		
		for(int i=0;i<table.getColumnCount();i++)
			table.getColumn(i).pack();
		
/*		
		try {
			DocumentBuilderFactory dbFactory = DocumentBuilderFactory.newInstance();
			DocumentBuilder dBuilder = dbFactory.newDocumentBuilder();
			Document doc = dBuilder.parse(queryResult);
			System.out.println(doc.getChildNodes().getLength());
		}
		catch (Exception e) {
			e.printStackTrace();
		}*/
		
		
/*		for(int i=1;i<=10;i++) {
			TableColumn column=new TableColumn(table,SWT.NONE);
			column.setWidth(100);
			column.setText("Column "+i);
		}
		for(int i=1;i<=10;i++) {
			TableItem item=new TableItem(table,SWT.NONE);
			String[] items=new String[10];
			for(int j=0;j<10;j++) {
				items[j]=new Integer((int)(Math.random()*100)).toString();
						
			}
			item.setText(items);
		} */
		
		
	}

	public void setFocus() {
		// TODO Auto-generated method stub

	}

}
