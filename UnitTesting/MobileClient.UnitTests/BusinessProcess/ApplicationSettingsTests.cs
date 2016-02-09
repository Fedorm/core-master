using System;
using BitMobile.Application;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MoblieClient.Tests.BusinessProcess
{
    [TestClass]
    public class ApplicationSettingsTests
    {
        [TestMethod]
        public void ParseApplicationString_AllArgs_SettingsChanged()
        {
            ApplicationSettings_Stub settings = new ApplicationSettings_Stub();
            PrivateObject obj = new PrivateObject(settings, new PrivateType(typeof(ApplicationSettings)));

            object actual = obj.Invoke("ParseArguments", "app -t -d -c -s -dw -nowebdav -backgroundloaddisabled -re:kugushew@gmail.com");

            Assert.AreEqual("app", actual);
            Assert.IsTrue(settings.DevelopModeEnabled);
            Assert.IsTrue(settings.TestAgentEnabled);
            Assert.IsTrue(settings.ForceClearCache);
            Assert.IsTrue(settings.SyncOnStart);
            Assert.IsTrue(settings.BackgoundLoadDisabled);
            Assert.IsTrue(settings.WaitDebuggerEnabled);
            Assert.IsTrue(settings.WebDavDisabled);
            Assert.AreEqual("kugushew@gmail.com", settings.DevelopersEmail);
        }

        class ApplicationSettings_Stub : ApplicationSettings
        {
            public override ApplicationSettings ReadSettings()
            {
                return this;
            }

            public override void WriteSettings()
            {

            }
        }
    }
}
