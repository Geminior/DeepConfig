namespace DeepConfigEditor.Components
{
    using System.Windows;
    using System.Windows.Controls;

    public class SimpleButtonList : ListBox
    {
        protected override System.Windows.DependencyObject GetContainerForItemOverride()
        {
            return new Button();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is Button;
        }
    }
}
