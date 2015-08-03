namespace DeepConfigEditor.Spec
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using FakeItEasy;
    using FluentAssertions;
    using DeepConfigEditor.ViewModels;
    using Caliburn.Micro;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.TestTypes;
    using DeepConfigEditor.Messages;

    [TestClass]
    public class ShellViewModelTests
    {
        [TestMethod]
        public void Handling_view_requests_should_work()
        {
            /* Arrange */
            var mocks = new TestAssistant().MockAll();
            var main = A.Dummy<MainViewModel>();

            A.CallTo(() => mocks.ViewModelFactory.Get(typeof(DummyViewModel))).Returns(new DummyViewModel());

            var optionsReq = new ViewRequest(typeof(DummyViewModel));
            var vm = new ShellViewModel(main, mocks.Messenger, mocks.ViewModelFactory);
            var handler = vm as IHandle<ViewRequest>;

            /* Act */
            (vm as IActivate).Activate();
            handler.Handle(optionsReq);

            /* Assert */
            A.CallTo(() => mocks.ViewModelFactory.Get(typeof(DummyViewModel))).MustHaveHappened(Repeated.Exactly.Once);
            vm.ActiveItem.Should().BeOfType<DummyViewModel>();
        }

        [TestMethod]
        public void Handling_view_request_to_other_than_main_if_main_is_not_active_should_ignore_and_log()
        {
            /* Arrange */
            var mocks = new TestAssistant().MockAll();
            var main = A.Dummy<MainViewModel>();
            var active = new object();

            var optionsReq = new ViewRequest(typeof(DummyViewModel));
            var vm = new ShellViewModel(main, mocks.Messenger, mocks.ViewModelFactory);
            var handler = vm as IHandle<ViewRequest>;

            /* Act */
            vm.ActiveItem = active;
            handler.Handle(optionsReq);
            
            /* Assert */
            A.CallTo(() => mocks.Logger.Warn(A<string>.Ignored, A<object[]>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => mocks.ViewModelFactory.Get(typeof(DummyViewModel))).MustNotHaveHappened();
            vm.ActiveItem.Should().BeSameAs(active);
        }

        [TestMethod]
        public void Handling_main_request_with_main_already_active_should_work_but_not_create_a_new_instance()
        {
            /* Arrange */
            var mocks = new TestAssistant().MockAll();
            var main = A.Dummy<MainViewModel>();

            var vm = new ShellViewModel(main, mocks.Messenger, mocks.ViewModelFactory);
            var handler = vm as IHandle<ViewRequest>;
            vm.MonitorEvents();

            /* Act */
            (vm as IActivate).Activate();
            handler.Handle(ViewRequest.MainView);

            /* Assert */
            A.CallTo(() => mocks.ViewModelFactory.Get(typeof(DummyViewModel))).MustNotHaveHappened();
            vm.ActiveItem.Should().BeSameAs(main);
            vm.ShouldNotRaisePropertyChangeFor((svm) => svm.ActiveItem);
        }

        [TestMethod]
        public void Handling_main_request_with_main_NOT_already_active_should_work_but_not_create_a_new_instance()
        {
            /* Arrange */
            var mocks = new TestAssistant().MockAll();
            var main = A.Dummy<MainViewModel>();
            var active = new DummyViewModel();

            var vm = new ShellViewModel(main, mocks.Messenger, mocks.ViewModelFactory);
            var handler = vm as IHandle<ViewRequest>;

            vm.ActiveItem = active;
            vm.MonitorEvents();

            /* Act */
            handler.Handle(ViewRequest.MainView);

            /* Assert */
            A.CallTo(() => mocks.ViewModelFactory.Get(typeof(DummyViewModel))).MustNotHaveHappened();
            vm.ActiveItem.Should().BeSameAs(main);
            vm.ShouldRaisePropertyChangeFor((svm) => svm.ActiveItem);
        }

        //[TestMethod]
        //public void Handling_IRequestMain_request_with_main_already_active_should_work_but_not_create_a_new_instance()
        //{
        //    /* Arrange */
        //    var mocks = new TestAssistant().MockAll();
        //    var main = A.Dummy<MainViewModel>();
        //    var req = A.Dummy<IRequestMainView>();

        //    var vm = new ShellViewModel(main, mocks.Messenger, mocks.ViewModelFactory);
        //    var handler = vm as IHandle<IRequestMainView>;
        //    vm.MonitorEvents();

        //    /* Act */
        //    (vm as IActivate).Activate();
        //    handler.Handle(req);

        //    /* Assert */
        //    A.CallTo(() => mocks.ViewModelFactory.Get(typeof(DummyViewModel))).MustNotHaveHappened();
        //    vm.ActiveItem.Should().BeSameAs(main);
        //    vm.ShouldNotRaisePropertyChangeFor((svm) => svm.ActiveItem);
        //}

        //[TestMethod]
        //public void Handling_IRequestMain_request_with_main_NOT_already_active_should_work_but_not_create_a_new_instance()
        //{
        //    /* Arrange */
        //    var mocks = new TestAssistant().MockAll();
        //    var main = A.Dummy<MainViewModel>();
        //    var req = A.Dummy<IRequestMainView>();
        //    var active = new DummyViewModel();

        //    var vm = new ShellViewModel(main, mocks.Messenger, mocks.ViewModelFactory);
        //    var handler = vm as IHandle<IRequestMainView>;

        //    vm.ActiveItem = active;
        //    vm.MonitorEvents();

        //    /* Act */
        //    handler.Handle(req);

        //    /* Assert */
        //    A.CallTo(() => mocks.ViewModelFactory.Get(typeof(DummyViewModel))).MustNotHaveHappened();
        //    vm.ActiveItem.Should().BeSameAs(main);
        //    vm.ShouldRaisePropertyChangeFor((svm) => svm.ActiveItem);
        //}
    }
}
