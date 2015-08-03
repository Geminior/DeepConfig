namespace DeepConfigEditor.Components
{
    using System.Windows;
    using System.Windows.Controls;
    using DeepConfigEditor.Extensions;

    public class MenuItemContainerSelector : ItemContainerTemplateSelector
    {
        public ItemContainerTemplate SeparatorTemplate { get; set; }

        public ItemContainerTemplate ParentTemplate { get; set; }

        public ItemContainerTemplate ItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, ItemsControl parentItemsControl)
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

            if (data is ActionItemParent)
            {
                return ParentTemplate;
            }

            return ItemTemplate;
        }
    }
}
