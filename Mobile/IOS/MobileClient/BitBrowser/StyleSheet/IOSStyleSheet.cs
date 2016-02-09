using System;
using System.Collections.Generic;
using MonoTouch.UIKit;
using BitMobile.Controls;
using BitMobile.Controls.StyleSheet;
using System.Text.RegularExpressions;

namespace BitMobile.IOS
{
	public class IOSStyleSheet : StyleSheet
	{
		Dictionary<IStyledObject,Dictionary<Type,Style>> assignedStyles = new Dictionary<IStyledObject, Dictionary<Type, Style>> ();
		Dictionary<String,Dictionary<Type,Style>> stylesByKey = new Dictionary<String, Dictionary<Type, Style>> ();

		public IOSStyleSheet ()
		{
			Helper = new StyleHelper (this);
		}

		private String GetKey (IControl<UIView> control)
		{
			String s = "";
			if (control.Parent != null)
				s = GetKey ((IControl<UIView>)control.Parent);

			String cssClass = null;
			if (control is IStyledObject) {
				cssClass = control.CssClass;
				if (!String.IsNullOrEmpty (cssClass))
					cssClass = cssClass.ToLower ();
			}
			
			if (!String.IsNullOrEmpty (s))
				s = s + " ";
			
			if (!String.IsNullOrEmpty (cssClass)) {
				s = s + String.Format ("({0}|{1})", control.GetType ().Name.ToLower (), cssClass);
			} else
				s = s + control.GetType ().Name.ToLower ();

			return s;
		}

		public override void Assign (object root)
		{
			IControl<UIView> control = root as IControl<UIView>;
			if (control != null) {
				String viewKey = GetKey (control);
				if (!stylesByKey.ContainsKey (viewKey)) {
					stylesByKey.Add (viewKey, new Dictionary<Type, Style> ());
					foreach (var item in Styles) {
						String selector = item.Key;
						int cnt = selector.Split (' ').Length;
						
						String[] key = viewKey.Split (' ');
						if (cnt <= key.Length) {
							int idx = key.Length - 1;
							String pattern = WrapWord (key [idx]);
							while (--cnt > 0) {
								pattern = WrapWord (key [--idx]) + @"\s" + pattern;
							}
							
							Regex regex = new Regex (pattern);
							if (regex.Match (selector).Success) {
								foreach (Style style in item.Value) {
									stylesByKey [viewKey].Remove (style.GetType ());
									stylesByKey [viewKey].Add (style.GetType (), style);
								}
							}
							
						}				
					}
				}

				assignedStyles.Add (control, stylesByKey [viewKey]);
			}

			IContainer container = control as IContainer;
			if (container != null)
				foreach (object sv in container.Controls)
					Assign (sv);
		}

		public override Dictionary<Type, Style> GetStyles (object obj)
		{
			if (obj is IStyledObject)
			if (assignedStyles.ContainsKey ((IStyledObject)obj))
				return assignedStyles [(IStyledObject)obj];
			return new Dictionary<Type, Style> ();
		}

		string WrapWord (string s)
		{
			return string.Format (@"{0}{1}{0}", @"\b", s);
		}
	}
}

