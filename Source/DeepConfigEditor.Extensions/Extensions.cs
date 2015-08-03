namespace DeepConfigEditor.Extensions
{
    using System.Collections.Generic;

    /// <summary>
    /// Various utility extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Adds a range of children to an <see cref="ActionItemParent"/>
        /// </summary>
        /// <param name="p">The parent.</param>
        /// <param name="children">The soon to be children.</param>
        /// <returns>The parent</returns>
        public static ActionItemParent AddRange(this ActionItemParent p, params IActionItem[] children)
        {
            return AddRange(p, (IEnumerable<IActionItem>)children);
        }

        /// <summary>
        /// Adds a range of children to an <see cref="ActionItemParent"/>
        /// </summary>
        /// <param name="p">The parent.</param>
        /// <param name="children">The soon to be children.</param>
        /// <returns>The parent</returns>
        public static ActionItemParent AddRange(this ActionItemParent p, IEnumerable<IActionItem> children)
        {
            foreach (var c in children)
            {
                p.ChildActions.Add(c);
            }

            return p;
        }
    }
}
