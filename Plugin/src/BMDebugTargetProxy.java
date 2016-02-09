/*******************************************************************************
 * Copyright (c) 2006, 2007 IBM Corporation and others.
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 * 
 * Contributors:
 *     IBM Corporation - initial API and implementation
 *******************************************************************************/

import org.eclipse.debug.core.model.IDebugTarget;
import org.eclipse.debug.internal.ui.viewers.update.DebugEventHandler;
import org.eclipse.debug.internal.ui.viewers.update.DebugTargetEventHandler;
import org.eclipse.debug.internal.ui.viewers.update.DebugTargetProxy;

/**
 * @since 3.2
 *
 */
public class BMDebugTargetProxy extends DebugTargetProxy {

	/**
	 * @param target
	 */
	public BMDebugTargetProxy(IDebugTarget target) {
		super(target);
	}

	/* (non-Javadoc)
	 * @see org.eclipse.debug.internal.ui.viewers.update.DebugTargetProxy#createEventHandlers()
	 */
	protected DebugEventHandler[] createEventHandlers() {
		System.out.println("create event handlers");
		return new DebugEventHandler[] { new DebugTargetEventHandler(this), new BMThreadEventHandler(this) };
	}

}
