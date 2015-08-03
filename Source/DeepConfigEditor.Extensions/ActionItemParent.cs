namespace DeepConfigEditor.Extensions
{
    using System.Collections.Generic;
    using Caliburn.Micro;

    /// <summary>
    /// Represents an action parent, that is a container of actions. This type is only relevant for menu type actions.
    /// </summary>
    public class ActionItemParent : ActionItemBase
    {
        private IList<IActionItem> _childActions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionItemParent"/> class.
        /// </summary>
        /// <param name="context">The action context.</param>
        public ActionItemParent(IActionContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionItemParent"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="caption">The caption.</param>
        public ActionItemParent(IActionContext context, string caption)
            : base(context)
        {
            this.Caption = caption;
        }

        /// <summary>
        /// Gets the list of child actions.
        /// </summary>
        /// <value>
        /// The child actions.
        /// </value>
        public IList<IActionItem> ChildActions
        {
            get
            {
                if (_childActions == null)
                {
                    _childActions = new BindableCollection<IActionItem>();
                }

                return _childActions;
            }
        }
    }
}
