namespace DeepConfig.Spec.Utilities
{
    using System.Threading;
    using DeepConfig.Utilities;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SimpleCacheTests
    {
        [TestMethod]
        public void Adding_and_retrieving_an_item_should_work_as_expected()
        {
            /* Arrange */
            var c = new SimpleCache<string, string>();
            string key = "Val1";

            /* Act */
            c.Add(key, "TheValue", 10000, false);

            bool exists = c.Contains(key);
            string val = c[key];

            /* Assert */
            exists.Should().BeTrue();
            val.Should().Be("TheValue");
        }

        [TestMethod]
        public void Removing_an_item_should_work_as_expected()
        {
            /* Arrange */
            var c = new SimpleCache<string, string>();
            string key = "Val1";

            /* Act */
            c.Add(key, "TheValue", 10000, false);
            c.Remove(key);
            bool exists = c.Contains(key);
            string val = c[key];

            /* Assert */
            exists.Should().BeFalse();
            val.Should().BeNull();
        }

        [TestMethod]
        public void Clearing_cache_should_work_as_expected()
        {
            /* Arrange */
            var c = new SimpleCache<string, string>();

            /* Act */
            c.Add("Val1", "TheValue", 10000, false);
            c.Add("Val2", "TheValue2", 10000, false);

            int countPre = c.Count;
            c.Clear();
            int countPost = c.Count;

            /* Assert */
            countPre.Should().Be(2);
            countPost.Should().Be(0);
        }

        [TestMethod]
        public void Basic_expiration_should_work_as_expected()
        {
            /* Arrange */
            var c = new SimpleCache<string, string>();
            string key = "Val1";

            /* Act */
            c.Add(key, "TheValue", 10, false);

            Thread.Sleep(11);

            bool exists = c.Contains(key);
            string val = c[key];

            /* Assert */
            exists.Should().BeFalse();
            val.Should().BeNull();
        }

        [TestMethod]
        public void Sliding_expiration_should_work_as_expected()
        {
            /* Arrange */
            var c = new SimpleCache<string, string>();
            string key = "Val1";

            /* Act */
            c.Add(key, "TheValue", 10, true);

            Thread.Sleep(5);
            string val1 = c[key];

            Thread.Sleep(5);
            string val2 = c[key];

            Thread.Sleep(11);
            string val3 = c[key];

            /* Assert */
            val1.Should().Be("TheValue");
            val2.Should().Be("TheValue");
            val3.Should().BeNull();
        }

        [TestMethod]
        public void Auto_cleanup_should_work_as_expected()
        {
            /* Arrange */
            var c = new SimpleCache<string, string>(15);
            string key = "Val1";

            /* Act */
            c.Add(key, "TheValue", 10, true);
            int countPre = c.Count;

            Thread.Sleep(20);
            string val = c[key];
            int countPost = c.Count;

            /* Assert */
            countPre.Should().Be(1);
            countPost.Should().Be(0);
            val.Should().BeNull();
        }
    }
}
