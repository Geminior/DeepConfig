namespace DeepConfig.Spec
{
    using System;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConfigErrorTests
    {
        [TestMethod]
        public void All_error_constructors_should_work_as_expected()
        {
            /* Arrange / Act */
            var ex = new Exception();
            var e1 = new ConfigError();
            var e2 = new ConfigError(ConfigErrorCode.FailedToLoad, "SomeMessage");
            var e3 = new ConfigError(ConfigErrorCode.FailedToSave, "SomeOtherMessage", ex);

            /* Assert */
            e1.Code.Should().Be(ConfigErrorCode.Unknown);
            e1.Message.Should().Be(null);
            e1.Exception.Should().Be(null);

            e2.Code.Should().Be(ConfigErrorCode.FailedToLoad);
            e2.Message.Should().Be("SomeMessage");
            e2.Exception.Should().Be(null);

            e3.Code.Should().Be(ConfigErrorCode.FailedToSave);
            e3.Message.Should().Be("SomeOtherMessage");
            e3.Exception.Should().Be(ex);
        }
    }
}
