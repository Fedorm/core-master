using BitMobile.Application;
using BitMobile.Controls;
using BitMobile.Controls.StyleSheet;
using BitMobile.Script;
using BitMobile.Utilities.Develop;
using BitMobile.Utilities.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BitMobile.Factory
{
    public class ScreenFactory
    {
        static ScreenFactory factory = null;
        static ObjectFactory objectFactory = new ObjectFactory();
        static Dictionary<String, StyleSheet> styleSheets = new Dictionary<string, StyleSheet>();

        public static ScreenFactory CreateInstance()
        {
            if (factory == null)
                factory = new ScreenFactory();
            return factory;
        }

        private ScreenFactory()
        {
        }

        public object CreateScreen<T>(String screenName, ValueStack.ValueStack stack, BitMobile.Controllers.ScreenController controller) where T : StyleSheet, new()
        {
            string TS_NAME = "CreateScreen: " + screenName;
			TimeStamp.Start ("Total");
            TimeStamp.Start(TS_NAME);

            System.IO.Stream screenStream = ApplicationContext.Context.DAL.GetScreenByName(screenName);
            if (screenStream == null)
                throw new Exception(String.Format("Can't load screen {0}", screenName));

            TimeStamp.Log(TS_NAME, "Prepare");
            TimeStamp.Start(TS_NAME);

            controller.OnLoading();

            TimeStamp.Log(TS_NAME, "OnLoading invoke");
            TimeStamp.Start(TS_NAME);

            IControl<object> scr = (IControl<object>)objectFactory.CreateObject(stack, screenStream);

            TimeStamp.Log(TS_NAME, "CreateInstance objects");
            TimeStamp.Start(TS_NAME);

            scr.CreateView(); // TODO: Replace to platform controllers

            TimeStamp.Log(TS_NAME, "CreateInstance views");
            TimeStamp.Start(TS_NAME);

            ApplyStyles(screenName, scr as IStyledObject, new T());

            TimeStamp.Log(TS_NAME, "Apply styles");
            TimeStamp.Start(TS_NAME);

            controller.OnLoad();

            TimeStamp.Log(TS_NAME, "OnLoad invoke");

			TimeStamp.Log ("Total");
            TimeStamp.WriteLog();
            TimeCollector.Write("CallFunction");
            TimeCollector.Stop("CallFunction");
            TimeCollector.Write("CallFunctionNoException");
            TimeCollector.Stop("CallFunctionNoException");

            return scr;
        }

        private void ApplyStyles(String screenName, IStyledObject scr, StyleSheet styleSheet)
        {
            String cssFile = null;
            if (scr is ICustomStyleSheet)
            {
                if (!String.IsNullOrEmpty(((ICustomStyleSheet)scr).StyleSheet))
                    cssFile = ((ICustomStyleSheet)scr).StyleSheet;
            }

            InitStyles(ref styleSheet, screenName, cssFile);

            styleSheet.Assign(scr);

            scr.Apply(styleSheet, new Bound(), new Bound());
        }

        private void InitStyles(ref StyleSheet styleSheet, String screenName, String cssFile)
        {
            if (!styleSheets.ContainsKey(screenName))
            {
                bool hasNotStyle = true;

                foreach (BitMobile.Configuration.DefaultStyle ds in ApplicationContext.Context.Configuration.Style.DefaultStyles.Controls)
                {
                    Stream cssStream = null;
                    if (ApplicationContext.Context.DAL.TryGetStyleByName(ds.File, out cssStream))
                    {
                        hasNotStyle = false;
                        styleSheet.Load(cssStream);
                    }
                    else
                        throw new ResourceNotFoundException("Style", ds.File);
                }

                if (!String.IsNullOrEmpty(cssFile))
                {
                    Stream cssStream = null;
                    if (ApplicationContext.Context.DAL.TryGetStyleByName(cssFile, out cssStream))
                    {
                        hasNotStyle = false;
                        styleSheet.Load(cssStream);
                    }
                }

                if (hasNotStyle)
                    throw new ResourceNotFoundException("Style", screenName);

                styleSheets.Add(screenName, styleSheet);
            }
            else
                styleSheet = styleSheets[screenName];
        }

    }
}

