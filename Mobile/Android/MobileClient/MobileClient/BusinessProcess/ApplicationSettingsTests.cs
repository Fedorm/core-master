using System;
using BitMobile.Application;
using System.Reflection;
using NUnit.Framework;
using MobileClient.Tests;

namespace MoblieClient.Tests.BusinessProcess
{
    [TestFixture]
    public class ApplicationSettingsTests
    {
        [Test]
        public void ParseApplicationString()
        {
            ApplicationSettings_Stub settings = new ApplicationSettings_Stub();
            PrivateObject obj = new PrivateObject(settings, typeof(ApplicationSettings));

            object actual = obj.Invoke("ParseArguments", "app -t -d -c -s");

            Assert.AreEqual("app", actual);
            Assert.IsTrue(settings.DevelopModeEnabled);
            Assert.IsTrue(settings.TestAgentEnabled);
            Assert.IsTrue(settings.ForceClearCache);
            Assert.IsTrue(settings.SyncOnStart);
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
