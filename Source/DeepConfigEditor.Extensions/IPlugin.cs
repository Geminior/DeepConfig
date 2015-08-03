namespace DeepConfigEditor.Extensions
{
    /// <summary>
    /// Marker interface which marks a given class as a plugin for the editor. Use this to mark ViewModels and also Views of <see cref="IProvideConfigSource"/>s if they reside in a different assembly than their matching ViewModel.
    /// </summary>
    public interface IPlugin
    {
    }
}
