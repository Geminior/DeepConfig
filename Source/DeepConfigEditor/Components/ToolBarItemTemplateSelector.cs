namespace DeepConfigEditor.Components
{
    using System.Windows;
    using System.Windows.Controls;
    using DeepConfigEditor.Extensions;

    public class ToolBarItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SeparatorTemplate { get; set; }

        public DataTemplate ItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject parentItemsControl)
        {
            var data = item as IActionItem;
            if (data == null)
            {
                return null;
            }

            if (data is ActionItemSeparator)
            {
                return SeparatorTemplate;
            }

            //This item type is not intended for the toolbar
            if (data is ActionItemParent)
            {
                return null;
            }

            return ItemTemplate;
        }
    }
}
