namespace DeepConfigEditor.ViewModels
{
    using System;
    using Caliburn.Micro;
    using DeepConfig;
    using DeepConfigEditor.Config;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Services;

    public class ShellViewModel : Conductor<object>.Collection.OneActive,
        IHandle<ViewRequest>, IHandle<ConfigSourceRequest>, IHandle<ShellMessage>, IHandle<ActiveConfigurationChangedMessage>
    {
        private readonly MainViewModel _main;
        private readonly AppearanceConfig _shellCfg;
        private readonly IViewModelFactory _viewModelFactory;

        public ShellViewModel(MainViewModel main, IEventAggregator messenger, IViewModelFactory viewModelFactory)
        {
            this.DisplayName = CommonRes.AppName;
            this.ActiveItem = _main = main;

            _viewModelFactory = viewModelFactory;

            //While this could also be injected, it serves no purpose as the config framework provides a testing interface, and this cfg is only used here
            _shellCfg = AppearanceConfig.Instance;

            messenger.Subscribe(this);
        }

        public AppearanceConfig ShellConfig
        {
            get { return _shellCfg; }
        }

        public bool AlwaysOnTop
        {
            get { return _shellCfg.AlwaysOnTop; }
            set { _shellCfg.AlwaysOnTop = value; }
        }

        public bool IsBusy
        {
            get;
            private set;
        }

        public ISupplyStatusInfo StatusInfo
        {
            get;
            private set;
        }

        private bool IsValidNavigation
        {
            get
            {
                if (!_main.IsActive || !object.ReferenceEquals(_main, this.ActiveItem))
                {
                    Logger.Instance.Warn("Attempted to change from one alternate view to another, this is not supported.");
                    return false;
                }

                return true;
            }
        }

        void IHandle<ViewRequest>.Handle(ViewRequest message)
        {
            if (message.RequestedViewModel == null)
            {
                Logger.Instance.Debug("Main requested 2");
                if (!_main.IsActive)
                {
                    Logger.Instance.Debug("Main request executed 2");
                    this.ChangeActiveItem(_main, true);
                }

                return;
            }

            if (!IsValidNavigation)
            {
                return;
            }

            object vm = message.RequestedViewModel;
            if (vm is Type)
            {
                vm = _viewModelFactory.Get((Type)vm);
            }

            this.ActivateItem(vm);
        }

        void IHandle<ConfigSourceRequest>.Handle(ConfigSourceRequest message)
        {
            if (!IsValidNavigation)
            {
                return;
            }

            Logger.Instance.Debug("Config source select requested");
            var vm = _viewModelFactory.GetConfigSourceSelectViewModel(message);

            this.ActivateItem(vm);
        }

        void IHandle<ShellMessage>.Handle(ShellMessage message)
        {
            switch (message.RequestedAction)
            {
                case ShellMessage.Action.StartWait:
                {
                    this.IsBusy = true;
                    NotifyOfPropertyChange(() => this.IsBusy);
                    Logger.Instance.Debug("Busy");
                    break;
                }

                case ShellMessage.Action.StopWait:
                {
                    this.IsBusy = false;
                    NotifyOfPropertyChange(() => this.IsBusy);
                    Logger.Instance.Debug("Not Busy");
                    break;
                }

                case ShellMessage.Action.Exit:
                {
                    TryClose();
                    break;
                }

                case ShellMessage.Action.ToggleAlwaysOnTop:
                {
                    this.AlwaysOnTop = !this.AlwaysOnTop;
                    NotifyOfPropertyChange(() => this.AlwaysOnTop);
                    break;
                }
            }
        }

        void IHandle<ActiveConfigurationChangedMessage>.Handle(ActiveConfigurationChangedMessage message)
        {
            if (!string.IsNullOrEmpty(message.Context.CurrentConfiguration.FriendlyName))
            {
                this.DisplayName = string.Format("{0} - [{1}]", CommonRes.AppName, message.Context.CurrentConfiguration.FriendlyName.Elipsify(40, true));
            }
            else
            {
                this.DisplayName = CommonRes.AppName;
            }

            NotifyOfPropertyChange(() => this.DisplayName);
        }

        protected override void OnActivationProcessed(object item, bool success)
        {
            if (success)
            {
                this.StatusInfo = (item as ISupplyStatusInfo) ?? EmptyStatus.Instance;
                NotifyOfPropertyChange(() => this.StatusInfo);
            }

            base.OnActivationProcessed(item, success);
        }

        protected override void OnDeactivate(bool close)
        {
            ConfigMaster.SetSettings(_shellCfg);
            ConfigMaster.SetSettings(EditorSettings.Instance);

            base.OnDeactivate(close);
        }

        private class EmptyStatus : ISupplyStatusInfo
        {
            private static EmptyStatus _instance = new EmptyStatus { StatusMessage = string.Empty, StatusState = string.Empty };

            public string StatusMessage
            {
                get;
                set;
            }

            public string StatusState
            {
                get;
                set;
            }

            internal static EmptyStatus Instance
            {
                get { return _instance; }
            }
        }
    }
}
