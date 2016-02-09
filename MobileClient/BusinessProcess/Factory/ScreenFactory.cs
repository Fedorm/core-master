using System;
using System.Collections.Generic;
using System.IO;
using BitMobile.Application;
using BitMobile.Application.Exceptions;
using BitMobile.Application.StyleSheet;
using BitMobile.BusinessProcess.SolutionConfiguration;
using BitMobile.Common.BusinessProcess.Controllers;
using BitMobile.Common.BusinessProcess.Factory;
using BitMobile.Common.Controls;
using BitMobile.Common.Develop;
using BitMobile.Common.StyleSheet;
using BitMobile.Common.ValueStack;

namespace BitMobile.BusinessProcess.Factory
{
    public class ScreenFactory : IScreenFactory
    {
        static ScreenFactory _factory;
        static readonly ObjectFactory ObjectFactory = new ObjectFactory();
        static readonly Dictionary<String, IStyleSheet> StyleSheets = new Dictionary<string, IStyleSheet>();

        public static ScreenFactory CreateInstance()
        {
            return _factory ?? (_factory = new ScreenFactory());
        }

        private ScreenFactory()
        {
        }

        public object CreateScreen(string screenName, IValueStack stack, IScreenController controller, object styleCache)
        {
            controller.SetCurrentScreenController();

            string tsName = "CreateScreen: " + screenName;

            TimeStamp.Start(tsName);

            var screenStream = ApplicationContext.Current.Dal.GetScreenByName(screenName);
            if (screenStream == null)
                throw new Exception(String.Format("Can't load screen {0}", screenName));

            TimeStamp.Log(tsName, "Prepare");
            TimeStamp.Start(tsName);

            controller.OnLoading(screenName);

            TimeStamp.Log(tsName, "OnLoading invoke");
            TimeStamp.Start(tsName);

            var scr = (IControl<object>)ObjectFactory.CreateObject(stack, screenStream);

            TimeStamp.Log(tsName, "Parse screen");
            TimeStamp.Start(tsName);

//            GC.Collect();
//            TimeStamp.Log(tsName, "GC collect");
//            TimeStamp.Start(tsName);

            scr.CreateView(); // TODO: Replace to platform controllers

            TimeStamp.Log(tsName, "Create views");

            ApplyStyles(screenName, scr, styleCache);

            TimeStamp.Start(tsName);

            controller.OnLoad(screenName);

            TimeStamp.Log(tsName, "OnLoad invoke");

            ApplicationContext.Current.Dal.ClearStringCache();

            TimeStamp.WriteAll();
            TimeCollector.WriteAll();

            return scr;
        }

        private void ApplyStyles(string screenName, ILayoutable scr, object cache)
        {
            string cssFile = null;
            var sheet = scr as ICustomStyleSheet;
            if (sheet != null)
            {
                if (!string.IsNullOrEmpty(sheet.StyleSheet))
                    cssFile = sheet.StyleSheet;
            }

            TimeStamp.Start("Init styles");

            IStyleSheet styleSheet = InitStyles(screenName, cssFile);
            styleSheet.SetCache((IDisposable)cache);

            TimeStamp.Log("Init styles");
            TimeStamp.Start("Assign styles");

            styleSheet.Assign(scr);

            TimeStamp.Log("Assign styles");
            TimeStamp.Start("Apply styles");

            scr.ApplyStyles(styleSheet, StyleSheetContext.Current.EmptyBound, StyleSheetContext.Current.EmptyBound);

            TimeStamp.Log("Apply styles");
        }

        private IStyleSheet InitStyles(string screenName, String cssFile)
        {
            IStyleSheet styleSheet;
            if (!StyleSheets.ContainsKey(screenName))
            {
                styleSheet = StyleSheetContext.Current.CreateStyleSheet();

                bool hasNotStyle = true;

                foreach (DefaultStyle ds in ApplicationContext.Current.Configuration.Style.DefaultStyles.Controls)
                {
                    Stream cssStream;
                    if (ApplicationContext.Current.Dal.TryGetStyleByName(ds.File, out cssStream))
                    {
                        hasNotStyle = false;
                        styleSheet.Load(cssStream);
                    }
                    else
                        throw new ResourceNotFoundException("Style", ds.File);
                }

                if (!string.IsNullOrEmpty(cssFile))
                {
                    Stream cssStream;
                    if (ApplicationContext.Current.Dal.TryGetStyleByName(cssFile, out cssStream))
                    {
                        hasNotStyle = false;
                        styleSheet.Load(cssStream);
                    }
                }

                if (hasNotStyle)
                    throw new ResourceNotFoundException("Style", screenName);

                StyleSheets.Add(screenName, styleSheet);
            }
            else
                styleSheet = StyleSheets[screenName];
            return styleSheet;
        }

    }
}

