namespace DeepConfig.Spec.Testing
{
    using DeepConfig.Testing;
    using DeepConfig.TestTypes;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestServiceTests
    {
        [TestMethod]
        public void Test_service_singleton_mapping_should_work()
        {
            /* Arrange */
            var cfg = new ConfigWithBasicProps();

            /* Act */
            cfg.MapSingletonInstance();

            var res1 = ConfigMaster.GetSettings<ConfigWithBasicProps>();

            cfg.UnmapSingletonInstance();

            var res2 = ConfigMaster.GetSettings<ConfigWithBasicProps>();

            /* Assert */
            res1.Should().BeSameAs(cfg);
            res2.Should().BeNull();
        }
    }
}
