namespace DeepConfigEditor.Extensions
{
    using Caliburn.Micro.Extras;

    /// <summary>
    /// Interface enabling display of popup dialogs
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows the specified message in a popup dialog.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="button">The buttons to show on the dialog.</param>
        /// <param name="icon">The icon to show on the dialog.</param>
        /// <returns>A result indicating which button the user pressed.</returns>
        MessageResult Show(string message, string caption, MessageButton button, MessageImage icon);
    }
}
