namespace DeepConfigEditor.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using Caliburn.Micro;
    using Caliburn.Micro.Extras;
    using DeepConfigEditor.Config;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Models;
    using DeepConfigEditor.Resources;
    using DeepConfigEditor.Services;

    public class ConfigurationViewModel : Screen, IHandle<ConfigRequest>
    {
        private readonly IEventAggregator _messenger;
        private readonly IDialogService _dialog;
        private readonly ITaskRunner _taskRunner;
        private readonly IActionContext _actionContext;
        private readonly RepeatGuard _repeatGuard;
        private ConfigSource _currentSource;
        private ConfigInfo _configInfo;
        private ConfigurationWrapper _configData;

        public ConfigurationViewModel(IEventAggregator messenger, IDialogService dialog, ITaskRunner taskRunner, IActionContext actionContext)
        {
            _messenger = messenger;
            _dialog = dialog;
            _taskRunner = taskRunner;
            _actionContext = actionContext;
            _repeatGuard = new RepeatGuard();
            _configData = new ConfigurationWrapper();

            _messenger.Subscribe(this);

            _configInfo = new ConfigInfo
            {
                Exists = true
            };
        }

        public ConfigurationWrapper ConfigData
        {
            get { return _configData; }
        }

        public override void CanClose(Action<bool> callback)
        {
            Logger.Instance.Debug("Attempting close");

            EnsureChangesSaved(callback);
        }

        public async void Handle(ConfigRequest message)
        {
            switch (message.RequestedAction)
            {
                case ConfigRequest.Action.Save:
                {
                    if (_currentSource == null)
                    {
                        _messenger.Publish(new ConfigSourceRequest(ConfigSourcePurpose.Save));
                        return;
                    }

                    Logger.Instance.Debug("Initiating save");
                    await SaveChangesAsync();
                    break;
                }

                case ConfigRequest.Action.SaveAs:
                {
                    Logger.Instance.Debug("Initiating save as");
                    await SaveChangesAsync(message.Source);
                    break;
                }

                case ConfigRequest.Action.Refresh:
                {
                    await RefreshAsync();
                    break;
                }

                case ConfigRequest.Action.Delete:
                {
                    await DeleteConfigAsync();
                    break;
                }
            }
        }

        internal Task<bool> LoadConfigAsync(ConfigSource source)
        {
            _currentSource = source;

            //Execute the load
            var t = new Task(
                () =>
                {
                    _configData.Load(_currentSource.Provider);
                    EditorSettings.Instance.AddRecentSource(_currentSource);
                },
                TaskCreationOptions.LongRunning);

            return _taskRunner.ExecuteWithDialogOnError(t, MessagesRes.ConfigLoadError);
        }

        protected override void OnActivate()
        {
            UpdateConfigInfo();
            Logger.Instance.Debug("Activated: " + _configInfo.FriendlyName);
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                Logger.Instance.Debug("Closed: " + _configInfo.FriendlyName);
                _messenger.Unsubscribe(this);
                UpdateConfigInfo(true);
            }

            base.OnDeactivate(close);
        }

        private async void EnsureChangesSaved(Action<bool> callback)
        {
            using (_repeatGuard)
            {
                if (_repeatGuard.IsBusy)
                {
                    callback(false);
                    return;
                }

                if (!_configData.HasChanges)
                {
                    callback(true);
                    return;
                }

                var response = _dialog.Show(MessagesRes.ConfigSavePrompt, CommonRes.PendingChanges, MessageButton.YesNoCancel, MessageImage.Question);

                if (response == MessageResult.Cancel)
                {
                    callback(false);
                    return;
                }
                else if (response == MessageResult.No)
                {
                    callback(true);
                    return;
                }
            }

            if (_currentSource != null)
            {
                var res = await SaveChangesAsync();
                callback(res);
            }
            else
            {
                //This action is executed after the user selects or cancels selection of a provider to save to
                Action<ConfigSource, bool> selectionCallback = async (s, sourceCancelled) =>
                {
                    if (sourceCancelled)
                    {
                        callback(false);
                        return;
                    }

                    var res = await SaveChangesAsync(s);
                    callback(res);
                };

                _messenger.Publish(
                new ConfigSourceRequest(ConfigSourcePurpose.Save, selectionCallback));
            }
        }

        private async Task<bool> SaveChangesAsync(ConfigSource newSource = null)
        {
            using (_repeatGuard)
            {
                if (_repeatGuard.IsBusy)
                {
                    return false;
                }

                var destination = newSource ?? _currentSource;

                if (destination.Provider.IsReadOnly)
                {
                    _dialog.Show(MessagesRes.ConfigSaveReadOnly, CommonRes.Error, MessageButton.OK, MessageImage.Error);
                    return false;
                }

                var t = new Task(
                    () =>
                    {
                        _configData.Save(destination.Provider);

                        if (destination != _currentSource)
                        {
                            _currentSource = destination;
                            EditorSettings.Instance.AddRecentSource(_currentSource);
                        }

                        UpdateConfigInfo();

                        Logger.Instance.Debug("Save called: " + _currentSource.DisplayName);
                    },
                    TaskCreationOptions.LongRunning);

                return await _taskRunner.ExecuteWithDialogOnError(t, MessagesRes.ConfigSaveError);
            }
        }

        private async Task DeleteConfigAsync()
        {
            bool res = false;

            using (_repeatGuard)
            {
                if (_repeatGuard.IsBusy)
                {
                    return;
                }

                if (_currentSource == null)
                {
                    return;
                }

                string delMsg = string.Format(MessagesRes.ConfigDeleteWarning, _configInfo.FriendlyName);
                var response = _dialog.Show(delMsg, CommonRes.Confirm, MessageButton.YesNo, MessageImage.Question);

                if (response != MessageResult.Yes)
                {
                    return;
                }

                var t = new Task(
                    () =>
                    {
                        _configData.Delete(_currentSource.Provider);
                        EditorSettings.Instance.RemoveRecentSource(_currentSource);
                    });

                res = await _taskRunner.ExecuteWithDialogOnError(t, MessagesRes.ConfigDeleteError);
            }

            if (res)
            {
                this.TryClose();
            }
        }

        private async Task RefreshAsync()
        {
            using (_repeatGuard)
            {
                if (_repeatGuard.IsBusy)
                {
                    return;
                }

                if (_currentSource == null)
                {
                    return;
                }

                if (_configData.HasChanges)
                {
                    var response = _dialog.Show(MessagesRes.ConfigRefreshWarning, CommonRes.Confirm, MessageButton.YesNo, MessageImage.Question);

                    if (response != MessageResult.Yes)
                    {
                        return;
                    }
                }

                await LoadConfigAsync(_currentSource);

                UpdateConfigInfo();
            }
        }

        private void UpdateConfigInfo(bool close = false)
        {
            if (close)
            {
                _configInfo = null;
            }
            else
            {
                _configInfo.IsNew = (_currentSource == null);

                if (_configInfo.IsNew)
                {
                    _configInfo.CanDelete = false;
                    _configInfo.FriendlyName = CommonRes.NewConfigName;
                }
                else
                {
                    var p = _currentSource.Provider;
                    _configInfo.CanDelete = p.CanDelete;
                    _configInfo.IsReadOnly = p.IsReadOnly;
                    _configInfo.FriendlyName = _currentSource.DisplayName;
                }
            }

            _actionContext.CurrentConfiguration = _configInfo;
            _messenger.PublishOnUIThreadAsync(new ActiveConfigurationChangedMessage(_actionContext));
        }

        //All actions take place on the UI thread so there should be no need to synchronize
        private class RepeatGuard : IDisposable
        {
            private int _entryCount;

            public bool IsBusy
            {
                get
                {
                    return _entryCount++ > 0;
                }
            }

            public void Dispose()
            {
                _entryCount--;
            }
        }
    }
}
