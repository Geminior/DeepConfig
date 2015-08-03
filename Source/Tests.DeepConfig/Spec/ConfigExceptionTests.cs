namespace DeepConfig.Spec
{
    using System;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConfigExceptionTests
    {
        [TestMethod]
        public void All_exception_ctors_should_work_as_expected()
        {
            /* Arrange / Act */
            var inner = new Exception();
            var err1 = new ConfigError(ConfigErrorCode.InvalidNode, "SomeError");
            var err2 = new ConfigError(ConfigErrorCode.InvalidNode, "SomeOtherError");

            var ex1 = new ConfigException("SomeMessage1", ConfigErrorCode.MissingGetter);
            var ex2 = new ConfigException("SomeMessage2", ConfigErrorCode.MissingSetter, inner);
            var ex3 = new ConfigException(null);
            var ex4 = new ConfigException(new[] { err1, err2 });

            /* Assert */
            ex1.Message.Should().Be("SomeMessage1");
            ex1.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.MissingGetter);
            ex1.InnerException.Should().BeNull();

            ex2.Message.Should().Be("SomeMessage2");
            ex2.Errors.Should().ContainSingle(e => e.Code == ConfigErrorCode.MissingSetter);
            ex2.InnerException.Should().BeSameAs(inner);

            ex3.Message.Should().Be(string.Empty);
            ex3.Errors.Should().BeEmpty();
            ex3.InnerException.Should().BeNull();

            ex4.Message.Should().Be("SomeError\r\nSomeOtherError");
            ex4.Errors.Should().HaveCount(2);
            ex4.InnerException.Should().BeNull();
        }
    }
}
