namespace DeepConfigEditor
{
    using System.Windows;
    using Caliburn.Micro;
    using DeepConfig;
    using DeepConfig.Testing;
    using DeepConfigEditor;
    using DeepConfigEditor.Actions;
    using DeepConfigEditor.Config;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Services;
    using DeepConfigEditor.Utilities;
    using FakeItEasy;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Ninject;

    public class TestAssistant
    {
        private IKernel _container;

        public TestAssistant()
        {
            _container = new StandardKernel();

            IoC.GetInstance = (service, key) => _container.Get(service, key);
            IoC.GetAllInstances = (service) => _container.GetAll(service);
            IoC.BuildUp = (instance) => _container.Inject(instance);

            BindConfigs();
        }

        [AssemblyInitialize]
        public void EnsureUriSchema()
        {
            //Ensure that the pack and application parts of a pack url get registered
            var tmp = Application.Current;
        }

        public MocksAll MockAll()
        {
            //Bind the base type that others depend on
            var msg = A.Fake<IEventAggregator>();
            var proc = A.Fake<IProcessService>();
            var winMgr = A.Fake<IWindowManager>();

            _container.Bind<IWindowManager>().ToConstant(winMgr);
            _container.Bind<IEventAggregator>().ToConstant(msg);
            _container.Bind<IProcessService>().ToConstant(proc);

            //The action context does not need to be mocked since it has no functionality, its just a state store
            _container.Bind<IActionContext>().To<ActionContext>().InSingletonScope();
            var actionCtx = _container.Get<IActionContext>();
            
            //Also the actions manager need not have its dependencies satisfied in the fake, since if the fake is used we are not testing the actions manager
            var actionMgr = A.Fake<IActionsManager>();
            var ioc = A.Fake<IKernel>(o => o.Wrapping(_container));

            _container.Bind<IActionsManager>().ToConstant(actionMgr);
            _container.Bind<IKernel>().ToConstant(ioc);

            Logger.Instance = A.Fake<ILogger>();

            return new MocksAll
            {
                WindowManager = winMgr,
                Messenger = msg,
                ActionContext = actionCtx,
                ActionsManager = actionMgr,
                Logger = Logger.Instance,
                ProcessUtil = proc,
                Container = ioc,
                ViewModelFactory = A.Fake<IViewModelFactory>()
            };
        }

        private void BindConfigs()
        {
            //Not really needed to explicitly set since the manager would load an empty config anyway, but this makes it clear.
            TestService.MapSingletonInstance(new PluginConfig());
        }

        public class MocksAll
        {
            public IWindowManager WindowManager { get; set; }

            public IEventAggregator Messenger { get; set; }

            public IActionContext ActionContext { get; set; }

            public IActionsManager ActionsManager { get; set; }

            public IProcessService ProcessUtil { get; set; }

            public IViewModelFactory ViewModelFactory { get; set; }

            public IKernel Container { get; set; }

            public ILogger Logger { get; set; }
        }
    }
}
