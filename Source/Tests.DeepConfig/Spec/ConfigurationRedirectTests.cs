namespace DeepConfig.Spec
{
    using DeepConfig.Providers;
    using FakeItEasy;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConfigurationRedirectTests
    {
        [TestMethod]
        public void If_no_redirect_is_present_trying_to_get_it_should_return_null()
        {
            /* Arrange */
            var mgr = A.Fake<IConfigManager>();
            A.CallTo(() => mgr.GetSettings<ConfigurationRedirect>()).Returns(null);

            /* Act */
            IConfigProvider p;
            bool res = ConfigurationRedirect.TryGetRedirect(mgr, out p);

            /* Assert */
            res.Should().BeFalse();
            p.Should().BeNull();
        }

        [TestMethod]
        public void If_a_redirect_is_present_trying_to_get_it_should_return_the_new_provider()
        {
            /* Arrange */
            var newProvider = A.Dummy<IConfigProvider>();
            var redir = new ConfigurationRedirect
            {
                ActualProvider = newProvider
            };

            var mgr = A.Fake<IConfigManager>();
            A.CallTo(() => mgr.GetSettings<ConfigurationRedirect>()).Returns(redir);

            /* Act */
            IConfigProvider p;
            bool res = ConfigurationRedirect.TryGetRedirect(mgr, out p);

            /* Assert */
            res.Should().BeTrue();
            p.Should().BeSameAs(newProvider);
        }
    }
}
