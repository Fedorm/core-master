/*******************************************************************************
 * Copyright (c) 2000, 2006 IBM Corporation and others.
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *
 * Contributors:
 *     IBM Corporation - initial API and implementation
 *     Joe Bowbeer (jozart@blarg.net) - removed dependency on runtime compatibility layer (bug 74528)
 *******************************************************************************/

import org.eclipse.core.resources.IResource;
import org.eclipse.core.runtime.IAdaptable;
import org.eclipse.jface.viewers.ISelection;
import org.eclipse.jface.viewers.IStructuredSelection;
import org.eclipse.ui.IEditorInput;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IFileEditorInput;
import org.eclipse.ui.IWorkbench;
import org.eclipse.ui.IWorkbenchPage;
import org.eclipse.ui.IWorkbenchWindow;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.console.ConsolePlugin;
import org.eclipse.ui.console.IConsole;
import org.eclipse.ui.console.IConsoleManager;
import org.eclipse.ui.console.MessageConsole;
import org.eclipse.ui.plugin.AbstractUIPlugin;

public class BitMobilePlugin extends AbstractUIPlugin {
    
    public static final String PLUGIN_ID = "BitMobile"; //$NON-NLS-1$
	public static final String ID_BM_DEBUG_MODEL = "BitMobile.debugModel";
    
    /**
     * Default instance of the receiver
     */ 
    private static BitMobilePlugin inst;

    /**
     * Creates the Readme plugin and caches its default instance
     */
    public BitMobilePlugin() {
        if (inst == null)
            inst = this;        
        System.setProperty("BitMobile.isDebugging", "false");
        System.setProperty("BitMobile.isSuspended", "false");
    }

    /**
     * Gets the plugin singleton.
     *
     * @return the default ReadmePlugin instance
     */
    static public BitMobilePlugin getDefault() {
        return inst;
    }
    
    public static IResource extractSelection(ISelection sel) {
	      if (!(sel instanceof IStructuredSelection))
	         return null;
	      IStructuredSelection ss = (IStructuredSelection) sel;
	      Object element = ss.getFirstElement();
	      if (element instanceof IResource)
	         return (IResource) element;
	      if (!(element instanceof IAdaptable))
	         return null;
	      IAdaptable adaptable = (IAdaptable)element;
	      Object adapter = adaptable.getAdapter(IResource.class);
	      return (IResource) adapter;
	   }
	
	 public static IResource extractResource(IEditorPart editor) {
	      IEditorInput input = editor.getEditorInput();
	      if (!(input instanceof IFileEditorInput))
	         return null;
	      return ((IFileEditorInput)input).getFile();
	   }
	    
  public static IResource getActiveResource() {
		IWorkbench iWorkbench = PlatformUI.getWorkbench();
		IWorkbenchWindow iWorkbenchWindow = iWorkbench.getActiveWorkbenchWindow();
		if (iWorkbenchWindow==null) 
			return null;
		IWorkbenchPage iWorkbenchPage = iWorkbenchWindow.getActivePage();
		if (iWorkbenchPage==null) 
			return null;
		IResource resource=extractSelection(iWorkbenchPage.getSelection());
		if (resource==null) {
			IEditorPart editorPart=iWorkbenchPage.getActiveEditor();
			if(editorPart!=null) 
				resource=extractResource(editorPart);
		}
		return resource;
  }
  
  
	public static void printToConsole(String message) {
		findConsole("Bitmobile console").newMessageStream().println(message);
	}
	

	 private static MessageConsole findConsole(String name) {
	      ConsolePlugin plugin = ConsolePlugin.getDefault();
	      IConsoleManager conMan = plugin.getConsoleManager();
	      IConsole[] existing = conMan.getConsoles();
	      for (int i = 0; i < existing.length; i++)
	         if (name.equals(existing[i].getName()))
	            return (MessageConsole) existing[i];
	      //no console found, so create a new one
	      MessageConsole myConsole = new MessageConsole(name, null);
	      conMan.addConsoles(new IConsole[]{myConsole});
	      return myConsole;
	   }
  
    

}
