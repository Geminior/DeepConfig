namespace DeepConfigEditor.Services
{
    using System;
    using System.Threading.Tasks;
    using Caliburn.Micro;
    using Caliburn.Micro.Extras;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Messages;
    using DeepConfigEditor.Resources;

    public sealed class TaskRunner : ITaskRunner
    {
        private readonly IEventAggregator _messenger;
        private readonly IDialogService _dialog;

        public TaskRunner(IEventAggregator messenger, IDialogService dialog)
        {
            _messenger = messenger;
            _dialog = dialog;
        }

        public async Task<bool> ExecuteWithDialogOnError(Task t, string messageOnError)
        {
            try
            {
                _messenger.Publish(new ShellMessage(ShellMessage.Action.StartWait));

                if (t.Status == TaskStatus.Created)
                {
                    t.Start();
                }

                await t;

                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex);
                _dialog.Show(MessagesRes.ConfigLoadError, CommonRes.Error, MessageButton.OK, MessageImage.Error);

                return false;
            }
            finally
            {
                _messenger.Publish(new ShellMessage(ShellMessage.Action.StopWait));
            }
        }
    }
}
