namespace DeepConfigEditor.Services
{
    using System;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.ViewModels;

    public interface IViewModelFactory
    {
        T Get<T>();

        object Get(Type vmType);

        ConfigSourceSelectViewModel GetConfigSourceSelectViewModel(ConfigSourceRequest request);

        ConfigurationViewModel GetConfigurationViewModel();
    }
}
