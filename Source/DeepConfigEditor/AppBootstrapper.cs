namespace DeepConfigEditor
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using Caliburn.Micro;
    using Caliburn.Micro.Extras;
    using DeepConfig;
    using DeepConfig.Providers;
    using DeepConfigEditor.Actions;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Services;
    using DeepConfigEditor.Utilities;
    using DeepConfigEditor.ViewModels;
    using DeepConfigEditor.ViewModels.ConfigSourceSelection;
    using DeepConfigEditor.ViewModels.Options;
    using Ninject;

    public class AppBootstrapper : Bootstrapper<ShellViewModel>
    {
        private IKernel _container;

        protected override void Configure()
        {
            //Instantiate the ioc container
            _container = new StandardKernel();

            //Start by registering the known types for use, starting with Singletons
            _container.Bind<IWindowManager>().To<WindowManager>().InSingletonScope();
            _container.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            _container.Bind<IActionContext>().To<ActionContext>().InSingletonScope();
            _container.Bind<IActionsManager>().To<ActionsManager>().InSingletonScope();
            _container.Bind<IProcessService>().To<ProcessService>().InSingletonScope();
            _container.Bind<IDialogService>().To<DialogService>().InSingletonScope();
            _container.Bind<IViewModelFactory>().To<ViewModelFactory>().InSingletonScope();
            _container.Bind<ITaskRunner>().To<TaskRunner>().InSingletonScope();
            _container.Bind<IKernel>().ToConstant(_container);

            //Transients
            _container.Bind<IFileService>().To<FileService>().InTransientScope();

            //Register known config selection sources
            _container.Bind<IProvideConfigSource>().To<FileSourceViewModel>().InTransientScope();
            _container.Bind<IProvideConfigSource>().To<DatabaseSourceViewModel>().InTransientScope();

            //Register known options sources
            _container.Bind<IEditorOptions>().To<GeneralViewModel>().InTransientScope();
            _container.Bind<IEditorOptions>().To<ReferencePathsViewModel>().InTransientScope();
            _container.Bind<IEditorOptions>().To<PluginsViewModel>().InTransientScope();

            //Explicit self transients, just to be clear
            _container.Bind<ApplicationManager>().ToSelf().InTransientScope();

            //Register known probable assemblies (the current assembly is added automatically by Caliburn)
            AssemblySource.Instance.Add(typeof(ApplicationManager).Assembly);

            //Set the config provider to a user specific provider
            var cfgProvider = new UserFileConfigProvider(UserFileConfigProvider.UserStore.Roaming);
            ConfigMaster.ProviderMapping.MapDefaultProvider(cfgProvider);

            //Set the logger wrapper
            Logger.Instance = new Logger(NLog.LogManager.GetLogger("MainLogger"));

            //Enable Caliburn micro logging (Debugging bindings etc)
            //LogManager.GetLog = (t) => Logger.Instance;
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            var manager = _container.Get<ApplicationManager>();

            //We do initialization here in order to enable handling various command args without going through the entire init process only to possibly shut down again after having completed the requested action
            var postStartupAction = manager.Initialize(_container, e.Args);

            if (postStartupAction != null)
            {
                base.OnStartup(sender, e);

                postStartupAction();
            }
            else
            {
                Application.Shutdown();
            }
        }

        protected override void OnUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //TODO: implement messagebox alert and shutdown
            Logger.Instance.Error("Unhandled exception: ", e.Exception);

            e.Handled = true;
            Application.Shutdown();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.Get(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAll(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.Inject(instance);
        }
    }
}
