namespace DeepConfigEditor.Components
{
    using System.Windows;
    using System.Windows.Controls;

    public static class ElementExtensions
    {
        public static Window GetWindow(this FrameworkElement element)
        {
            if (element == null)
            {
                return null;
            }

            if (element is Window)
            {
                return (Window)element;
            }

            if (element.Parent == null)
            {
                return null;
            }

            return GetWindow(element.Parent as FrameworkElement);
        }
    }
}
