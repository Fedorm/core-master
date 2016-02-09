using Android.Views;
using BitMobile.Controls;
using BitMobile.Controls.StyleSheet;
using System;
using System.Collections.Generic;

namespace BitMobile.Droid
{
    public class AndroidStyleSheet : StyleSheet
    {
        Dictionary<IStyledObject, Dictionary<Type, Style>> assignedStyles = new Dictionary<IStyledObject, Dictionary<Type, Style>>();

        Dictionary<String, Dictionary<Type, Style>> stylesByKey = new Dictionary<String, Dictionary<Type, Style>>();

        public AndroidStyleSheet()
        {
            Helper = new StyleHelper(this);        
        }

        String GetKey(IControl<View> control)
        {
            String s = "";
            if (control.Parent is IControl<View>)
                s = GetKey((IControl<View>)control.Parent);

            String cssClass = null;
            if (control is IStyledObject)
            {
                cssClass = (control as IStyledObject).CssClass;
                if (!String.IsNullOrEmpty(cssClass))
                    cssClass = cssClass.ToLower();
            }

            if (!String.IsNullOrEmpty(s))
                s = s + " ";

            if (!String.IsNullOrEmpty(cssClass))
                s = s + String.Format("({0}|{1})", control.GetType().Name.ToLower(), cssClass);
            else
                s = s + control.GetType().Name.ToLower();

            return s;
        }
        
        public override void Assign(object root)
        {
            assignedStyles.Clear();

            AssignControl(root);
        }

        public override Dictionary<Type, Style> GetStyles(object obj)
        {
            if (obj is IStyledObject)
                if (assignedStyles.ContainsKey((IStyledObject)obj))
                    return assignedStyles[(IStyledObject)obj];
            return new Dictionary<Type, Style>();
        }

        void AssignControl(object obj)
        {
            IControl<View> view = obj as IControl<View>;
            if (view == null)
                throw new ArgumentException("Cannot assign this control");

            if (view is IStyledObject)
            {
                String viewKey = GetKey(view);
                if (!stylesByKey.ContainsKey(viewKey))
                {
                    stylesByKey.Add(viewKey, new Dictionary<Type, Style>());
                    foreach (var item in Styles)
                    {
                        String selector = item.Key;
                        int cnt = selector.Split(' ').Length;

                        String[] key = viewKey.Split(' ');
                        if (cnt <= key.Length)
                        {
                            int idx = key.Length - 1;
                            String pattern = WrapWord(key[idx]);
                            while (--cnt > 0)
                            {
                                pattern = WrapWord(key[--idx]) + @"\s" + pattern;
                            }

                            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern);
                            if (regex.Match(selector).Success)
                            {
                                foreach (Style style in item.Value)
                                {
                                    stylesByKey[viewKey].Remove(style.GetType());
                                    stylesByKey[viewKey].Add(style.GetType(), style);
                                }
                            }

                        }
                    }
                }

                assignedStyles.Add((IStyledObject)view, stylesByKey[viewKey]);
            }

            IContainer container = view as IContainer;
            if (container != null)
                foreach (object child in container.Controls)
                    AssignControl(child);
        }

        private String WrapWord(String s)
        {
            return String.Format(@"{0}{1}{0}", @"\b", s);
        }       
    }
}

