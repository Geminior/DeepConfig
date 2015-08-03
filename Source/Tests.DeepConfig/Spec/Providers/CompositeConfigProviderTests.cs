namespace DeepConfig.Spec.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using DeepConfig.Providers;
    using FakeItEasy;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CompositeConfigProviderTests
    {
        [TestMethod]
        public void Ctor_null_arg_should_throw()
        {
            /* Arrange / Act */
            Action a1 = () => new CompositeConfigProvider((IEnumerable<IConfigProvider>)null);
            Action a2 = () => new CompositeConfigProvider((IConfigProvider[])null);

            /* Assert */
            a1.ShouldThrow<ArgumentNullException>();
            a2.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void Constant_props_should_return_expected_values()
        {
            /* Arrange */
            IConfigProvider p = new CompositeConfigProvider();

            /* Assert */
            p.CanDelete.Should().BeFalse();
            p.IsReadOnly.Should().BeTrue();
        }

        [TestMethod]
        public void Source_dentifier_should_return_correct_value()
        {
            /* Arrange */
            var p1 = A.Fake<IConfigProvider>();
            A.CallTo(() => p1.SourceIdentifier).Returns("p1id");

            var p2 = A.Fake<IConfigProvider>();
            A.CallTo(() => p2.SourceIdentifier).Returns("p2id");

            string expectedId = "p1id_p2id";

            /* Act */
            var providers = new List<IConfigProvider> { p1, p2 };

            var p = new CompositeConfigProvider(providers);

            /* Assert */
            p.SourceIdentifier.Should().Be(expectedId);
        }

        [TestMethod]
        public void Unsupported_methods_should_throw()
        {
            /* Arrange */
            var doc = TestConfigFactory.CreateConfig();
            IConfigProvider p = new CompositeConfigProvider();

            /* Act */
            Action a1 = () => p.SaveConfig(doc);
            Action a2 = () => p.DeleteConfig();

            /* Assert */
            a1.ShouldThrow<NotSupportedException>();
            a2.ShouldThrow<NotSupportedException>();
        }

        [TestMethod]
        public void Initializing_configs_should_work()
        {
            /* Arrange */
            var p1 = A.Fake<IConfigProvider>();
            var p2 = A.Fake<IConfigProvider>();

            /* Act */
            IConfigProvider p = new CompositeConfigProvider(p1, p2);

            p.Initialize();

            /* Assert */
            A.CallTo(() => p1.Initialize()).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => p2.Initialize()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [TestMethod]
        public void Loading_configs_should_work()
        {
            /* Arrange */
            var p1 = A.Fake<IConfigProvider>();
            A.CallTo(() => p1.LoadConfig()).Returns(new XDocument[] { new XDocument() });

            var p2 = A.Fake<IConfigProvider>();
            A.CallTo(() => p2.LoadConfig()).Returns(null);

            var p3 = A.Fake<IConfigProvider>();
            A.CallTo(() => p3.LoadConfig()).Returns(new XDocument[] { null, new XDocument() });

            /* Act */
            var p = new CompositeConfigProvider(p1, p2);
            p.Providers.Add(p3);

            var docs = p.LoadConfig().ToList();

            /* Assert */
            docs.Should().HaveCount(2);
            A.CallTo(() => p1.LoadConfig()).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => p2.LoadConfig()).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => p3.LoadConfig()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [TestMethod]
        public void Modified_since_load_should_work_as_expected()
        {
            /* Arrange */
            var p1 = A.Fake<IConfigProvider>();
            A.CallTo(() => p1.ModifiedSinceLoad).Returns(false);

            var p2 = A.Fake<IConfigProvider>();
            A.CallTo(() => p2.ModifiedSinceLoad).Returns(true);

            var p3 = A.Fake<IConfigProvider>();
            A.CallTo(() => p3.ModifiedSinceLoad).Returns(false);

            /* Act */
            var pc1 = new CompositeConfigProvider(p1, p2, p3);
            var pc2 = new CompositeConfigProvider(p1, p3);

            var mod1 = pc1.ModifiedSinceLoad;
            var mod2 = pc2.ModifiedSinceLoad;

            /* Assert */
            A.CallTo(() => p1.ModifiedSinceLoad).MustHaveHappened(Repeated.Exactly.Twice);
            A.CallTo(() => p2.ModifiedSinceLoad).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => p3.ModifiedSinceLoad).MustHaveHappened(Repeated.Exactly.Once);

            mod1.Should().BeTrue();
            mod2.Should().BeFalse();
        }
    }
}
