namespace DeepConfigEditor.Services
{
    using System;
    using Caliburn.Micro;
    using DeepConfig.Providers;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.ViewModels;
    using Ninject;

    public class ViewModelFactory : IViewModelFactory
    {
        private readonly IEventAggregator _messenger;
        private readonly IActionContext _actionContext;
        private readonly IDialogService _dialog;
        private readonly ITaskRunner _taskRunner;
        private readonly IKernel _container;

        public ViewModelFactory(IEventAggregator messenger, IDialogService dialog, ITaskRunner taskRunner, IActionContext actionContext, IKernel container)
        {
            _messenger = messenger;
            _dialog = dialog;
            _taskRunner = taskRunner;
            _actionContext = actionContext;
            _container = container;
        }

        public ConfigurationViewModel GetConfigurationViewModel()
        {
            return new ConfigurationViewModel(_messenger, _dialog, _taskRunner, _actionContext);
        }

        public ConfigSourceSelectViewModel GetConfigSourceSelectViewModel(ConfigSourceRequest request)
        {
            var vm = _container.Get<ConfigSourceSelectViewModel>();

            vm.Initialize(request);

            return vm;
        }

        public T Get<T>()
        {
            return _container.Get<T>();
        }

        public object Get(Type vmType)
        {
            return _container.Get(vmType);
        }
    }
}
