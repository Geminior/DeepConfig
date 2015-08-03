namespace DeepConfigEditor.Services
{
    using System;
    using System.Threading.Tasks;
    using Caliburn.Micro;
    using Caliburn.Micro.Extras;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.ViewModels;

    public class DialogService : IDialogService
    {
        private IWindowManager _windowManager;

        public DialogService(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public MessageResult Show(string message, string caption, MessageButton button, MessageImage icon)
        {
            var vm = new DialogViewModel
            {
                Title = caption,
                Message = message,
                Buttons = button,
                Icon = icon
            };

            _windowManager.ShowDialog(vm);

            return vm.Result;
        }
    }
}
