using System;
using BitMobile.Application;
using BitMobile.Common.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitMobile.MobileClient.UnitTests.BusinessProcess
{
    [TestClass]
    public class ApplicationSettingsTests
    {
        [TestMethod]
        public void ParseApplicationString_AllArgs_SettingsChanged()
        {
            var settings = new ApplicationSettingsStub();
            var obj = new PrivateObject(settings, new PrivateType(typeof(ApplicationSettings)));

            object actual = obj.Invoke("ParseArguments", "app -t -d -c -s -dw -nowebdav" +
                " -backgroundloaddisabled -oldformatter -re:kugushew@gmail.com -loglifetime:10 -logmincount:50000" +
                " -disableping -disablepush");

            Assert.AreEqual("app", actual);
            Assert.IsTrue(settings.DevelopModeEnabled);
            Assert.IsTrue(settings.TestAgentEnabled);
            Assert.IsTrue(settings.ForceClearCache);
            Assert.IsTrue(settings.SyncOnStart);
            Assert.IsTrue(settings.BackgoundLoadDisabled);
            Assert.IsTrue(settings.WaitDebuggerEnabled);
            Assert.IsTrue(settings.WebDavDisabled);
            Assert.IsTrue(settings.BitMobileFormatterDisabled);
            Assert.AreEqual("kugushew@gmail.com", settings.DevelopersEmail);
            Assert.AreEqual(new TimeSpan(10, 0, 0, 0), settings.LogLifetime);
            Assert.AreEqual(50000, settings.LogMinCount);
        }

        class ApplicationSettingsStub : ApplicationSettings
        {
            public override IApplicationSettings ReadSettings()
            {
                return this;
            }

            public override void WriteSettings()
            {

            }
        }
    }
}
