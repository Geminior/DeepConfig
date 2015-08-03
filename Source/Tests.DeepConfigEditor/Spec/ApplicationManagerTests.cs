namespace DeepConfigEditor.Spec
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using FakeItEasy;
    using FluentAssertions;
    using System.Linq;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Extensions;
    using DeepConfig.Providers;
    using DeepConfigEditor.Config;
    using Caliburn.Micro;
    using Ninject;
    using Ninject.Activation;
    using System.Windows.Threading;
    
    [TestClass]
    public class ApplicationManagerTests
    {
        [TestMethod]
        public void Initialize_with_no_args_should_go_to_source_open()
        {
            /* Arrange */
            var mocks = new TestAssistant().MockAll();
            var mgr = new ApplicationManager(mocks.ActionsManager);

            /* Act */
            var a = mgr.Initialize(mocks.Container, new string[0]);

            a();

            /* Assert */
            A.CallTo(() => mocks.Messenger.Publish(A<ConfigSourceRequest>.That.Matches(r => r.RequestPurpose == ConfigSourcePurpose.Open))).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => mocks.ActionsManager.BuildActions(A<IEnumerable<IProvideActions>>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => mocks.ActionsManager.ExecuteCommandLine(A<string[]>.Ignored)).MustNotHaveHappened();
        }

        [TestMethod]
        public void Initialize_with_file_arg_should_go_to_request_display_of_config()
        {
            /* Arrange */
            var mocks = new TestAssistant().MockAll();
            var mgr = new ApplicationManager(mocks.ActionsManager);
            ConfigRequest msg = null;

            A.CallTo(() => mocks.Messenger.Publish(A<object>.Ignored)).Invokes(c => msg = c.Arguments[0] as ConfigRequest);

            /* Act */
            var a = mgr.Initialize(mocks.Container, new string[] { "SomeFileName" });

            a();

            /* Assert */
            msg.Should().NotBeNull();
            msg.RequestedAction.Should().Be(ConfigRequest.Action.Open);
            msg.Source.Provider.As<FileConfigProvider>().ConfigFileName.Should().Be("SomeFileName");

            A.CallTo(() => mocks.ActionsManager.BuildActions(A<IEnumerable<IProvideActions>>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => mocks.ActionsManager.ExecuteCommandLine(A<string[]>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [TestMethod]
        public void Initialize_with_command_arg_should_return_null()
        {
            /* Arrange */
            var mocks = new TestAssistant().MockAll();
            var mgr = new ApplicationManager(mocks.ActionsManager);

            A.CallTo(() => mocks.ActionsManager.ExecuteCommandLine(A<string[]>.Ignored)).Returns(true);

            /* Act */
            var a = mgr.Initialize(mocks.Container, new string[] { "MustHaveAtleastOneArg" });

            /* Assert */
            a.Should().BeNull();

            A.CallTo(() => mocks.ActionsManager.BuildActions(A<IEnumerable<IProvideActions>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => mocks.ActionsManager.ExecuteCommandLine(A<string[]>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [TestMethod]
        public void Initialize_with_an_existing_plugin_should_find_it_and_load_it()
        {
            /* Arrange */
            var mocks = new TestAssistant().MockAll();
            var mgr = new ApplicationManager(mocks.ActionsManager);

            PluginConfig.Instance.PluginFolder = "Addins";

            /* Act */
            var preInit = mocks.Container.TryGet<IProvideConfigSource>();

            mgr.Initialize(mocks.Container, new string[0]);

            var postInit = mocks.Container.TryGet<IProvideConfigSource>();

            /* Assert */
            AssemblySource.Instance.Should().ContainSingle(asm => asm.GetAssemblyName() == "SampleEditorPlugin");
            A.CallTo(mocks.Logger).MustNotHaveHappened();

            //Faking the IKernel doesn't work so instead we check the actual container
            preInit.Should().BeNull();
            postInit.Should().NotBeNull();
        }
    }
}
