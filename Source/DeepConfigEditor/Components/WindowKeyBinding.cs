namespace DeepConfigEditor.Components
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using Caliburn.Micro;
    using DeepConfigEditor.Extensions;
    using DeepConfigEditor.Services;
    using DeepConfigEditor.Utilities;

    /// <summary>
    /// Attaches keybindings associated with actions to the window, to enable global keyboard shortcuts.
    /// </summary>
    public class WindowKeyBinding : FrameworkElement
    {
        public static readonly DependencyProperty ActionsProperty =
            DependencyProperty.Register("Actions", typeof(IEnumerable<IAction>), typeof(WindowKeyBinding), new FrameworkPropertyMetadata(OnActionsChanged));

        private IEnumerable<ActionKeyBinding> _bindings;

        public WindowKeyBinding()
        {
            this.IsHitTestVisible = false;
            this.Unloaded += UnloadedHander;
        }

        public object Actions
        {
            get { return GetValue(WindowKeyBinding.ActionsProperty); }
            set { SetValue(WindowKeyBinding.ActionsProperty, value); }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == UIElement.IsEnabledProperty && _bindings != null)
            {
                var enabled = (bool)e.NewValue;

                _bindings.Apply(b => b.IsEnabled = enabled);
            }

            base.OnPropertyChanged(e);
        }

        private static void OnActionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var actions = e.NewValue as IEnumerable<IAction>;

            ((WindowKeyBinding)d).UpdateBindings(actions);
        }

        private void UpdateBindings(IEnumerable<IAction> actions)
        {
            if (Execute.InDesignMode)
            {
                return;
            }

            var parentWindow = this.GetWindow();
            if (parentWindow == null)
            {
                Logger.Instance.Warn("WindowKeyBinding defined with no window in the visual hierachy.");
                return;
            }

            var currentBindings = parentWindow.InputBindings;

            if (_bindings != null)
            {
                _bindings.Apply(b => currentBindings.Remove(b.KeyBinding));
            }

            if (actions != null)
            {
                //We don't want to create new ones every time this is iterated over.
                _bindings = ActionKeyBinding.From(actions).ToList();
                _bindings.Apply(a => currentBindings.Add(a.KeyBinding));
            }
            else
            {
                _bindings = null;
            }
        }

        private void UnloadedHander(object sender, RoutedEventArgs e)
        {
            UpdateBindings(null);
        }
    }
}
