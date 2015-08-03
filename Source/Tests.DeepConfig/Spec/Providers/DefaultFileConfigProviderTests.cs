namespace DeepConfig.Spec.Providers
{
    using System;
    using DeepConfig.Providers;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DefaultFileConfigProviderTests
    {
        [TestMethod]
        public void Default_config_file_must_be_found()
        {
            /* Arrange / Act */
            var p = new DefaultFileConfigProvider();

            /* Assert */
            //Not exactly a meaningful test, but just to not have to check for missing code coverage
            p.ConfigFileName.Should().Be(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
        }
    }
}
