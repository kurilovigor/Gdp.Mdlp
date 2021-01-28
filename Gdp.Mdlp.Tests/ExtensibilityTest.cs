using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gdp.Mdlp.Tests
{
    [TestClass]
    public class ExtensibilityTest
    {
        [TestMethod]
        public void TestConfiguration()
        {
            var config = Gdp.Mdlp.Service.ServiceConfiguration.Load("config.json");
            foreach (var factory in config.AddonFactories)
                foreach (var addon in factory.Addons)
                {

                    Type t = Type.GetType(addon.Type);
                    Assert.IsNotNull(t, $"Invalid type: {addon.Type}");
                }
        }
    }
}
