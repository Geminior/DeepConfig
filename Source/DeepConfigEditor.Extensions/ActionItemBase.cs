namespace DeepConfigEditor.Extensions
{
    using Caliburn.Micro;

    /// <summary>
    /// Base class for actions.
    /// </summary>
    public abstract class ActionItemBase : PropertyChangedBase, IActionItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionItemBase"/> class.
        /// </summary>
        /// <param name="context">The action context.</param>
        public ActionItemBase(IActionContext context)
        {
            this.Context = context;
            this.IsVisible = true;
        }

        /// <summary>
        /// Gets or sets the caption.
        /// </summary>
        /// <value>
        /// The caption.
        /// </value>
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets the tooltip.
        /// </summary>
        /// <value>
        /// The tooltip.
        /// </value>
        public string Tooltip { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the related control should be visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if it should be visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>
        /// The context that provides various services the action can use
        /// </value>
        protected IActionContext Context
        {
            get;
            private set;
        }
    }
}
