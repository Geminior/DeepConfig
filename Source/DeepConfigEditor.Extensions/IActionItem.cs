namespace DeepConfigEditor.Extensions
{
    /// <summary>
    /// Represents an action that can be shown in the application's menu or toolbar
    /// </summary>
    public interface IActionItem
    {
        /// <summary>
        /// Gets or sets the caption.
        /// </summary>
        /// <value>
        /// The caption.
        /// </value>
        string Caption { get; set; }

        /// <summary>
        /// Gets or sets the tooltip.
        /// </summary>
        /// <value>
        /// The tooltip.
        /// </value>
        string Tooltip { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the related control should be visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if it should be visible; otherwise, <c>false</c>.
        /// </value>
        bool IsVisible { get; set; }
    }
}
