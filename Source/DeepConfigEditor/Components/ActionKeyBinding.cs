namespace DeepConfigEditor.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Services;

    public class ActionKeyBinding : ICommand
    {
        private IAction _action;

        private ActionKeyBinding(IAction action, KeyGesture gesture)
        {
            _action = action;
            this.IsEnabled = true;
            this.KeyBinding = new KeyBinding(this, gesture);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }

            remove { CommandManager.RequerySuggested -= value; }
        }

        public KeyBinding KeyBinding
        {
            get;
            private set;
        }

        public bool IsEnabled
        {
            get;
            set;
        }

        public static ActionKeyBinding From(IAction action)
        {
            var g = ParseGesture(action, new KeyGestureConverter());

            if (g != null)
            {
                return new ActionKeyBinding(action, g);
            }

            return null;
        }

        public static IEnumerable<ActionKeyBinding> From(IEnumerable<IAction> actions)
        {
            var converter = new KeyGestureConverter();

            return from a in actions
                   where !string.IsNullOrEmpty(a.InputGesture)
                   let g = ParseGesture(a, converter)
                   where g != null
                   select new ActionKeyBinding(a, g);
        }

        public bool CanExecute(object parameter)
        {
            return this.IsEnabled && _action.CanExecute;
        }

        public void Execute(object parameter)
        {
            _action.Execute();
        }

        private static KeyGesture ParseGesture(IAction action, KeyGestureConverter converter)
        {
            try
            {
                return converter.ConvertFromString(action.InputGesture) as KeyGesture;
            }
            catch
            {
                Logger.Instance.Warn("Unable to create keybinding for action: {0}", action.InputGesture);
            }

            return null;
        }

        //private void OnCanExecuteChanged()
        //{
        //    var e = CanExecuteChanged;
        //    if(e != null)
        //    {
        //        e(this, EventArgs.Empty);
        //    }
        //}
    }
}
