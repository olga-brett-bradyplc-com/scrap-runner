using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brady.ScrapRunner.Mobile.Tests
{
    using AutoMapper;

    [TestClass]
    public class ScrapRunnerProfileTest
    {
        [TestMethod]
        public void AutoMapper_Configuration_IsValid()
        {
            Mapper.Initialize(m => m.AddProfile<ScrapRunnerMapperProfile>());
            Mapper.AssertConfigurationIsValid();
        }
    }
}
