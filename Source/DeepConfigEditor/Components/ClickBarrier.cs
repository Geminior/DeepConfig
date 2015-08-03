namespace DeepConfigEditor.Components
{
    using System.Windows;
    using System.Windows.Controls.Primitives;

    public class ClickBarrier : ButtonBase
    {
        static ClickBarrier()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ClickBarrier), new FrameworkPropertyMetadata(typeof(ClickBarrier)));
        }

        public ClickBarrier()
        {
            this.Click += HandleClick;
        }

        private void HandleClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
