using System;
using System.Collections.Generic;

namespace BitMobile.Controls
{
	public class ScreenData
	{
        public String Name { get; private set; }
        public String ControllerName { get; private set; }
        public BitMobile.Controls.IScreen Screen { get; private set; }
		
		public ScreenData(String name, String controllerName, BitMobile.Controls.IScreen screen)
		{
			this.Name = name;
            this.ControllerName = controllerName;
			this.Screen = screen;
		}
	}

}

