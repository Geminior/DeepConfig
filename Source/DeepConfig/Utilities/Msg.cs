namespace DeepConfig.Utilities
{
    using System.Globalization;

    /// <summary>
    /// To satisfy CA1305
    /// </summary>
    internal static class Msg
    {
        internal static string Text(string message, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, message, args);
        }
    }
}
