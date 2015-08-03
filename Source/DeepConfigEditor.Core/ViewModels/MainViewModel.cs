namespace DeepConfigEditor.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Caliburn.Micro;
    using DeepConfigEditor.Actions;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Services;

    public class MainViewModel : Conductor<Screen>, ISupplyStatusInfo, IHandle<ConfigRequest>, IHandle<ActiveConfigurationChangedMessage>
    {
        private IActionsManager _actions;
        private IEventAggregator _messenger;
        private IViewModelFactory _viewModelFactory;

        public MainViewModel(IActionsManager actions, IEventAggregator messenger, IViewModelFactory viewModelFactory)
        {
            _actions = actions;
            _messenger = messenger;
            _viewModelFactory = viewModelFactory;

            _messenger.Subscribe(this);

            this.KeybindingsEnabled = true;
        }

        public IEnumerable<IActionItem> MenuItems
        {
            get
            {
                return _actions.MenuItems;
            }
        }

        public IEnumerable<IActionItem> ToolBarItems
        {
            get
            {
                return _actions.ToolBarItems;
            }
        }

        public IEnumerable<IAction> Actions
        {
            get
            {
                return _actions.AllExecutables;
            }
        }

        public bool KeybindingsEnabled
        {
            get;
            private set;
        }

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

        public void Handle(ConfigRequest message)
        {
            //Handling of open and new are done here, and the check to ensure pending changes are saved is conducted by the close check on the child
            switch (message.RequestedAction)
            {
                case ConfigRequest.Action.New:
                {
                    Logger.Instance.Debug("Opening new config");
                    var vm = _viewModelFactory.GetConfigurationViewModel();
                    this.ActivateItem(vm);
                    break;
                }

                case ConfigRequest.Action.Open:
                {
                    Logger.Instance.Debug("Opening existing config");
                    Action<bool> openAction = async (okToLoad) =>
                    {
                        if (!okToLoad)
                        {
                            return;
                        }

                        //At this point  always close the previous item regardless of whether the load succeeds
                        if (this.ActiveItem != null)
                        {
                            ChangeActiveItem(null, true);
                        }

                        var vm = _viewModelFactory.GetConfigurationViewModel();

                        var loaded = await vm.LoadConfigAsync(message.Source);
                        if (loaded)
                        {
                            this.ChangeActiveItem(vm, true);
                        }
                    };

                    //First we close the current item if it exists and let it run its shutdown process, which may be cancelled.
                    //If there is no active item, just load
                    if (this.ActiveItem != null)
                    {
                        this.ActiveItem.CanClose(openAction);
                    }
                    else
                    {
                        openAction(true);
                    }

                    break;
                }

                case ConfigRequest.Action.StartScreen:
                {
                    var vm = _viewModelFactory.Get<StartScreenViewModel>();
                    this.ActivateItem(vm);
                    break;
                }
            }
        }

        public void Handle(ActiveConfigurationChangedMessage message)
        {
            var cfgInfo = _actions.ActionContext.CurrentConfiguration;
            if (cfgInfo == null)
            {
                this.StatusMessage = string.Empty;
                this.StatusState = string.Empty;
            }
            else
            {
                this.StatusMessage = cfgInfo.FriendlyName;
                this.StatusState = cfgInfo.IsReadOnly ? CommonRes.ReadOnly : string.Empty;
            }

            NotifyOfPropertyChange(() => this.StatusMessage);
            NotifyOfPropertyChange(() => this.StatusState);
        }

        protected override void OnActivate()
        {
            Logger.Instance.Debug("Activating Main");
            this.KeybindingsEnabled = true;
            this.NotifyOfPropertyChange(() => KeybindingsEnabled);

            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            Logger.Instance.Debug("Deactivating Main");
            this.KeybindingsEnabled = false;
            this.NotifyOfPropertyChange(() => KeybindingsEnabled);

            base.OnDeactivate(close);
        }
    }
}
