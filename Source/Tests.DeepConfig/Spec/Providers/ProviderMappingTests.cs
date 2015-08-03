namespace DeepConfig.Spec.Providers
{
    using System;
    using DeepConfig.Providers;
    using DeepConfig.TestTypes;
    using FakeItEasy;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProviderMappingTests
    {
        [TestMethod]
        public void Mapping_a_type_to_a_provider_and_resolving_it_should_work()
        {
            /* Arrange */
            Type mapChange = null;
            bool defaultChange = false;

            Action<Type> onMapChange = (Type t) => mapChange = t;
            Action onDefaultChange = () => defaultChange = true;

            var target = typeof(ConfigComplex);
            var p = A.Dummy<IConfigProvider>();

            var map = new ProviderMapping(onMapChange, onDefaultChange);

            /* Act */
            var hasPre = map.HasMapping(target);
            var mapPre = map.ResolveProvider(target);

            map.Map<ConfigComplex>().To(p);

            var hasPost = map.HasMapping(target);
            var mapPost = map.ResolveProvider(target);

            /* Assert */
            hasPre.Should().BeFalse();
            mapPre.Should().BeNull();

            hasPost.Should().BeTrue();
            mapPost.Should().BeSameAs(p);

            mapChange.Should().Be(target);
            defaultChange.Should().BeFalse();
        }

        [TestMethod]
        public void Mapping_a_type_to_a_provider_factory_and_resolving_it_should_work()
        {
            /* Arrange */
            Type mapChange = null;
            bool defaultChange = false;

            Action<Type> onMapChange = (Type t) => mapChange = t;
            Action onDefaultChange = () => defaultChange = true;

            var target = typeof(ConfigComplex);
            var p = A.Dummy<IConfigProvider>();
            Func<Type, IConfigProvider> pfact = (Type t) =>
            {
                if (t == target)
                {
                    return p;
                }

                return null;
            };

            var map = new ProviderMapping(onMapChange, onDefaultChange);

            /* Act */
            var hasPre = map.HasMapping(target);
            var mapPre = map.ResolveProvider(target);

            map.Map<ConfigComplex>().To(pfact);

            var hasPost = map.HasMapping(target);
            var mapPost = map.ResolveProvider(target);

            /* Assert */
            hasPre.Should().BeFalse();
            mapPre.Should().BeNull();

            hasPost.Should().BeTrue();
            mapPost.Should().BeSameAs(p);

            mapChange.Should().Be(target);
            defaultChange.Should().BeFalse();
        }

        [TestMethod]
        public void Unmapping_a_type_should_work()
        {
            /* Arrange */
            Type mapChange = null;
            bool defaultChange = false;

            Action<Type> onMapChange = (Type t) => mapChange = t;
            Action onDefaultChange = () => defaultChange = true;

            var target = typeof(ConfigComplex);
            var p = A.Dummy<IConfigProvider>();

            var map = new ProviderMapping(onMapChange, onDefaultChange);

            /* Act */
            map.Map<ConfigComplex>().To(p);

            var hasPost = map.HasMapping(target);
            var mapPost = map.ResolveProvider(target);
            mapChange = null;

            map.Unmap<ConfigComplex>();

            var hasPostUnmap = map.HasMapping(target);
            var mapPostUnmap = map.ResolveProvider(target);

            /* Assert */
            hasPost.Should().BeTrue();
            mapPost.Should().BeSameAs(p);

            hasPostUnmap.Should().BeFalse();
            mapPostUnmap.Should().BeNull();

            mapChange.Should().Be(target);
            defaultChange.Should().BeFalse();
        }

        [TestMethod]
        public void Mapping_a_default_provider_and_resolving_it_should_work()
        {
            /* Arrange */
            Type mapChange = null;
            bool defaultChange = false;

            Action<Type> onMapChange = (Type t) => mapChange = t;
            Action onDefaultChange = () => defaultChange = true;

            var p = A.Dummy<IConfigProvider>();

            var map = new ProviderMapping(onMapChange, onDefaultChange);

            /* Act */
            var mapPre = map.GetDefaultProvider();

            map.MapDefaultProvider(p);

            var mapPost = map.GetDefaultProvider();

            /* Assert */
            mapPre.Should().BeOfType<DefaultFileConfigProvider>();

            mapPost.Should().BeSameAs(p);

            mapChange.Should().BeNull();
            defaultChange.Should().BeTrue();
        }

        [TestMethod]
        public void Mapping_a_default_provider_factory_and_resolving_it_should_work()
        {
            /* Arrange */
            Type mapChange = null;
            bool defaultChange = false;

            Action<Type> onMapChange = (Type t) => mapChange = t;
            Action onDefaultChange = () => defaultChange = true;

            var p = A.Dummy<IConfigProvider>();
            Func<IConfigProvider> pfact = () => p;

            var map = new ProviderMapping(onMapChange, onDefaultChange);

            /* Act */
            var mapPre = map.GetDefaultProvider();

            map.MapDefaultProvider(pfact);

            var mapPost = map.GetDefaultProvider();

            /* Assert */
            mapPre.Should().BeOfType<DefaultFileConfigProvider>();

            mapPost.Should().BeSameAs(p);

            mapChange.Should().BeNull();
            defaultChange.Should().BeTrue();
        }

        [TestMethod]
        public void Unmapping_a_default_provider_should_work()
        {
            /* Arrange */
            Type mapChange = null;
            bool defaultChange = false;

            Action<Type> onMapChange = (Type t) => mapChange = t;
            Action onDefaultChange = () => defaultChange = true;

            var p = A.Dummy<IConfigProvider>();

            var map = new ProviderMapping(onMapChange, onDefaultChange);

            /* Act */
            map.MapDefaultProvider(p);

            var mapPost = map.GetDefaultProvider();

            map.UnmapDefaultProvider();

            var mapPostUnmap = map.GetDefaultProvider();

            /* Assert */
            mapPost.Should().BeSameAs(p);

            mapPostUnmap.Should().BeOfType<DefaultFileConfigProvider>();

            mapChange.Should().BeNull();
            defaultChange.Should().BeTrue();
        }
    }
}
