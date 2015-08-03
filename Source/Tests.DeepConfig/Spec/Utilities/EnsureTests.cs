namespace DeepConfig.Spec.Utilities
{
    using System;
    using DeepConfig.Utilities;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EnsureTests
    {
        [TestMethod]
        public void Ensure_not_null_should_work()
        {
            /* Act */
            Action a1 = () => Ensure.ArgumentNotNull(null, "NullValue");
            Action a2 = () => Ensure.ArgumentNotNull(new object(), "Value");

            /* Assert */
            a1.ShouldThrow<ArgumentNullException>();
            a2.ShouldNotThrow();
        }

        [TestMethod]
        public void Ensure_not_null_or_empty_should_work()
        {
            /* Act */
            Action a1 = () => Ensure.ArgumentNotNullOrEmpty(null, "NullValue");
            Action a2 = () => Ensure.ArgumentNotNullOrEmpty(string.Empty, "Empty");
            Action a3 = () => Ensure.ArgumentNotNullOrEmpty("SomeString", "Value");

            /* Assert */
            a1.ShouldThrow<ArgumentException>();
            a2.ShouldThrow<ArgumentException>();
            a3.ShouldNotThrow();
        }
    }
}
