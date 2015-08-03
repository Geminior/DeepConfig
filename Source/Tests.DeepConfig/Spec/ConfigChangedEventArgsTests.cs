namespace DeepConfig.Spec
{
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConfigChangedEventArgsTests
    {
        [TestMethod]
        public void Instance_must_return_correct_name()
        {
            /* Arrange / Act */
            string expected = "SomeSection";
            var args = new ConfigChangedEventArgs
            {
                SectionName = expected,
                SectionType = typeof(object),
                Cause = ConfigChangedEventArgs.ChangeCause.SectionChanged
            };

            /* Assert */
            args.SectionName.Should().Be(expected);
            args.SectionType.Should().Be(typeof(object));
            args.Cause.Should().Be(ConfigChangedEventArgs.ChangeCause.SectionChanged);
        }
    }
}
