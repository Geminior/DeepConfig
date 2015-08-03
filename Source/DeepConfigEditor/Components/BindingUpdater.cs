namespace DeepConfigEditor.Components
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using Caliburn.Micro;
    using DeepConfigEditor.Messages;

    public class BindingUpdater : FrameworkElement, IHandle<EnsureAllBindingsRequest>
    {
        public BindingUpdater()
        {
            this.IsHitTestVisible = false;

            var m = IoC.Get<IEventAggregator>();
            m.Subscribe(this);

            this.Unloaded += (s, e) =>
            {
                m.Unsubscribe(this);
            };
        }

        public void Handle(EnsureAllBindingsRequest message)
        {
            //TODO: Ensure that all types of used controls are covered here
            var currentFocusElement = Keyboard.FocusedElement as DependencyObject;
            if (currentFocusElement == null)
            {
                return;
            }

            var txtBox = currentFocusElement as TextBox;
            if (txtBox != null)
            {
                var bex = BindingOperations.GetBindingExpressionBase(currentFocusElement, TextBox.TextProperty);
                if (bex != null)
                {
                    bex.UpdateSource();
                }

                return;
            }
        }
    }
}
