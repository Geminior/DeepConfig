namespace DeepConfig.Spec.Utilities
{
    using System.Globalization;
    using DeepConfig.Utilities;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MsgTests
    {
        [TestMethod]
        public void Creating_a_message_should_work()
        {
            /* Arrange */
            var template = "Some Message {0}";
            var expected = string.Format(CultureInfo.CurrentCulture, template, "test");

            /* Act */
            var actual = Msg.Text(template, "test");

            /* Assert */
            actual.Should().Be(expected);
        }
    }
}
