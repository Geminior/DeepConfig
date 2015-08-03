namespace DeepConfigEditor.Messages
{
    /// <summary>
    /// A message request to ensure bindings are complete on the active element before caarying out saves etc.
    /// </summary>
    public sealed class EnsureAllBindingsRequest
    {
        public static readonly EnsureAllBindingsRequest Instance = new EnsureAllBindingsRequest();
    }
}
